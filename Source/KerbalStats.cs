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
using System.Linq;
using UnityEngine;

namespace KerbalStats {
	[KSPAddon (KSPAddon.Startup.Instantly, true)]
	public class KerbalStats : MonoBehaviour
	{
		public static KerbalStats current { get; private set; }
		internal Dictionary<string, IKerbalExt> kerbalext_modules;

		struct KerbalPair {
			public ProtoCrewMember pcm;
			public KerbalExt ext;
			public KerbalPair (ProtoCrewMember pcm, KerbalExt ext)
			{
				this.pcm = pcm;
				this.ext = ext;
			}
		};

		Dictionary<string, KerbalExt> kerbals;
		List<KerbalPair> loading_kerbals;

		public KerbalExt this[ProtoCrewMember pcm]
		{
			get {
				KerbalExt ext;
				kerbals.TryGetValue (pcm.name, out ext);
				return ext;
			}
		}

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

		void addKerbal (ProtoCrewMember pcm)
		{
			KerbalExt ext = new KerbalExt ();
			ext.NewKerbal (pcm);
			SetExt (pcm, ext);
		}

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

		void ProcessLoadingKerbals ()
		{
			foreach (var pair in loading_kerbals) {
				kerbals[pair.pcm.name] = pair.ext;
			}
			loading_kerbals = null;
		}

		void onKerbalAdded (ProtoCrewMember pcm)
		{
			if (loading_kerbals != null) {
				ProcessLoadingKerbals ();
			}
			StartCoroutine (WaitAndAddKerbal (pcm));
		}

		void onKerbalRemoved (ProtoCrewMember pcm)
		{
			kerbals.Remove (pcm.name);
		}

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

		void onProtoCrewMemberSave (GameEvents.FromToAction<ProtoCrewMember,ConfigNode> action)
		{
			ProtoCrewMember pcm = action.from;
			//Debug.LogFormat ("[KerbalStats] saving ext for {0}", pcm.name);
			ConfigNode node = action.to;
			ConfigNode kerbalExt = node.AddNode ("KerbalExt");
			if (kerbals != null) {
				//Debug.Log ("    from kerbals");
				kerbals[pcm.name].Save (kerbalExt);
			} else if (loading_kerbals != null) {
				//Debug.Log ("    from loading_kerbals");
			} else {
				//Debug.Log ("    from the ether");
				KerbalExt ext = new KerbalExt ();
				ext.NewKerbal (pcm);
				ext.Save (kerbalExt);
			}
		}

		void onGameStateCreated (Game game)
		{
			kerbals = new Dictionary<string, KerbalExt>();
			if (loading_kerbals != null) {
				ProcessLoadingKerbals ();
			}
		}

		void LoadModules ()
		{
			kerbalext_modules = new Dictionary<string, IKerbalExt> ();
			var modules = ModuleLoader.LoadModules<IKerbalExt> (new Type[] {typeof (KerbalStats)});
			var parms = new object[] {this};
			foreach (var m in modules) {
				IKerbalExt kext;
				kext = (IKerbalExt) m.Invoke (parms);
				kerbalext_modules[kext.name] = kext;
				//Debug.LogFormat ("[KS] module: {0}", kext.name);
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
			var modules = kerbalext_modules.Values.ToList ();
			foreach (var m in modules) {
				m.Shutdown ();
			}
		}
	}

}
