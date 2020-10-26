/*
This file is part of KerbalStats.

KerbalStats is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

KerbalStats is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with KerbalStats.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalStats {
	/** Manage mapping between KSP kerbals and extended stats.
	 *
	 * This class takes care of all the work required to maintain
	 * extended stats for kerbals.
	 */
	[KSPAddon (KSPAddon.Startup.Instantly, true)]
	public class KerbalStats : MonoBehaviour
	{
		/** Reference to the singleton
		 */
		public static KerbalStats current { get; private set; }

		/** Cache for all the extended stats modules, indexed by module
		 * name
		 */
		internal Dictionary<string, IKerbalExt> kerbalext_modules;

		/** Tie the kerbal with the kerbal's extended stats.
		 *
		 * Used for delaying creating the kerbal map, possibly due to
		 * unfortunate order of operations during game loading (lost
		 * info). Possibly to allow all scenarios to load before
		 * processing the kerbals, and/or to deal with the mess around
		 * ProtoCrewMember creation (event fired before class members
		 * are filled in)
		 */
		struct KerbalPair {
			/** Reference to the kerbal
			 */
			public ProtoCrewMember pcm { get; private set; }
			/** The kerbal's extended stats
			 */
			public KerbalExt ext { get; private set; }
			public KerbalPair (ProtoCrewMember pcm, KerbalExt ext)
			{
				this.pcm = pcm;
				this.ext = ext;
			}
		};

		/** Map between kerbal name and the kerbal's extended stats
		 */
		Dictionary<string, KerbalExt> kerbals;
		/** Keep track of kerbals and their extended stats until the
		 * game "stabilizes" after game load (reasons forgotten).
		 * See KerbalPair.
		 */
		List<KerbalPair> loading_kerbals;

		/** Fetch a kerbal's extended stats during game load
		 */
		KerbalExt find_loading_kerbal (ProtoCrewMember pcm)
		{
			int count = loading_kerbals.Count;
			for (int i = 0; i < count; i++) {
				if (loading_kerbals[i].pcm == pcm) {
					return loading_kerbals[i].ext;
				}
			}
			return null;
		}

		/** Fetch a kerbal's extended stats at any time.
		 */
		public KerbalExt this[ProtoCrewMember pcm]
		{
			get {
				KerbalExt ext = null;
				if (kerbals != null) {
					kerbals.TryGetValue (pcm.name, out ext);
				}
				if (ext == null && loading_kerbals != null) {
					ext = find_loading_kerbal (pcm);
				}
				return ext;
			}
		}

		/** Create the mapping between kerbal and the kerbal's stats.
		 *
		 * Handles timing issues during game load. internal instead of
		 * private for KerbalStatsScenario. Don't use.
		 */
		internal void SetExt (ProtoCrewMember pcm, KerbalExt ext)
		{
			if (loading_kerbals != null) {
				//Debug.Log("    loading_kerbals");
				loading_kerbals.Add (new KerbalPair (pcm, ext));
			} else if (kerbals != null) {
				//Debug.Log("    kerbals");
				kerbals[pcm.name] = ext;
			}
		}

		/** Add a kerbal to the system
		 *
		 * The kerbal is given extended stats and stored in the
		 * database.
		 */
		void addKerbal (ProtoCrewMember pcm)
		{
			KerbalExt ext = new KerbalExt ();
			SetExt (pcm, ext);
			ext.NewKerbal (pcm);
		}

		/** Wait a frame before adding the kerbal
		 *
		 * This deals with Contract Configurator changing the kerbal's
		 * name after creating the kerbal. The change is made in the
		 * same frame, but after the creation event has been fired, thus
		 * the need to wait.
		 */
		IEnumerator WaitAndAddKerbal (ProtoCrewMember pcm)
		{
			yield return null;
			//Debug.LogFormat ("[KerbalStats] onKerbalAdded: {0} {1} {2}",
			//				 pcm.name, pcm.rosterStatus, pcm.type);
			if (kerbals.ContainsKey (pcm.name)) {
				//Debug.LogFormat ("    {0} already added", pcm.name);
				yield break;
			}
			addKerbal (pcm);
		}

		/** Tidy up when the game has stabilized after game load
		 */
		void ProcessLoadingKerbals ()
		{
			foreach (var pair in loading_kerbals) {
				kerbals[pair.pcm.name] = pair.ext;
			}
			loading_kerbals = null;
		}

		/** Event handler for when a new kerbal is added
		 */
		void onKerbalAdded (ProtoCrewMember pcm)
		{
			/** Ensure processing of loaded kerbals is done
			 */
			if (loading_kerbals != null) {
				ProcessLoadingKerbals ();
			}
			StartCoroutine (WaitAndAddKerbal (pcm));
		}

		/** Event handler for when a kerbal is removed
		 */
		void onKerbalRemoved (ProtoCrewMember pcm)
		{
			kerbals.Remove (pcm.name);
		}

		/** Load a kerbal's extended stats from the kerbal's roster node
		 *
		 * Kerbal creation is a messy process in KSP, so this is called
		 * at all sorts of times.
		 */
		void onProtoCrewMemberLoad (GameEvents.FromToAction<ProtoCrewMember,ConfigNode> action)
		{
			if (loading_kerbals == null) {
				loading_kerbals = new List<KerbalPair>();
			}
			//Debug.LogFormat ("[KerbalStats] onProtoCrewMemberLoad: {0}", action);
			ProtoCrewMember pcm = action.from;
			ConfigNode node = action.to;
			string name = pcm.name;
			if (name == null && node != null && node.HasValue ("name")) {
				// it turns out onProtoCrewMemberLoad is sometimes fired too
				// early (before ProtoCrewMember is filled in)
				name = node.GetValue ("name");
			}
			// Kerbals created on entering the astronaut complex do not have
			// a config node, and kerbals created before installing KS won't
			// have a KerbalExt
			if (node != null && node.HasNode ("KerbalExt")) {
				//Debug.LogFormat ("[KerbalStats] loading ext for {0}", name);
				var kerbal = node.GetNode ("KerbalExt");
				var ext = new KerbalExt ();
				ext.Load (pcm, kerbal);
				SetExt (pcm, ext);
			} else {
				//Debug.LogFormat ("[KerbalStats] creating ext for {0}", name);
				addKerbal (pcm);
			}
		}

		/** Save a kerbal's extended stats to the kerbal's roster node.
		 */
		void onProtoCrewMemberSave (GameEvents.FromToAction<ProtoCrewMember,ConfigNode> action)
		{
			ProtoCrewMember pcm = action.from;
			//Debug.LogFormat ("[KerbalStats] saving ext for {0}", pcm.name);
			ConfigNode node = action.to;
			ConfigNode kerbalExt = node.AddNode ("KerbalExt");
			KerbalExt ext = this[pcm];
			if (ext != null) {
				//Debug.Log ("    from kerbals or loading_kerbals");
				ext.Save (kerbalExt);
			} else {
				//Debug.Log ("    from the ether");
				ext = new KerbalExt ();
				ext.NewKerbal (pcm);
				ext.Save (kerbalExt);
			}
		}

		/** Tidy up after the game has loaded or been created.
		 */
		void onGameStateCreated (Game game)
		{
			kerbals = new Dictionary<string, KerbalExt>();
			if (loading_kerbals != null) {
				ProcessLoadingKerbals ();
			}
		}

		/** Find all usable classes that implement IKerbalExt.
		 *
		 * For a class to be usable, it must have a constructor that
		 * takes a single KerbalStats parameter. These are the modules
		 * that implement the various extended stats.
		 *
		 * An single instance of the module is created (using the
		 * provided contructor). All modules are treated as singletons.
		 */
		void LoadModules ()
		{
			kerbalext_modules = new Dictionary<string, IKerbalExt> ();
			var modules = ModuleLoader.LoadModules<IKerbalExt> (new Type[] {typeof (KerbalStats)});
			var parms = new object[] {this};
			foreach (var m in modules) {
				IKerbalExt kext;
				kext = (IKerbalExt) m.Invoke (parms);
				kerbalext_modules[kext.ModuleName] = kext;
				Debug.LogFormat ("[KS] module: {0}", kext.ModuleName);
			}
		}

		void Awake ()
		{
			GameObject.DontDestroyOnLoad(this);

			current = this;
			enabled = false;
			LoadModules ();

			GameEvents.onKerbalAdded.Add (onKerbalAdded);
			GameEvents.onKerbalRemoved.Add (onKerbalRemoved);
			GameEvents.onProtoCrewMemberLoad.Add (onProtoCrewMemberLoad);
			GameEvents.onProtoCrewMemberSave.Add (onProtoCrewMemberSave);
			GameEvents.onGameStateCreated.Add (onGameStateCreated);
		}

		void OnDestroy ()
		{
			GameEvents.onKerbalAdded.Remove (onKerbalAdded);
			GameEvents.onKerbalRemoved.Remove (onKerbalRemoved);
			GameEvents.onProtoCrewMemberLoad.Remove (onProtoCrewMemberLoad);
			GameEvents.onProtoCrewMemberSave.Remove (onProtoCrewMemberSave);
			GameEvents.onGameStateCreated.Remove (onGameStateCreated);
			foreach (var m in kerbalext_modules.Values) {
				m.Shutdown ();
			}
		}
	}

}
