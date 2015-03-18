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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny {
	public class ProgenyTracker : IKerbalExt
	{
		Dictionary <string, Female> female_kerbals;
		Dictionary <string, Male> male_kerbals;
		internal static ProgenyTracker instance;

		Dictionary <string, Vessel> kerbal_vessels;
		public void AddKerbal (ProtoCrewMember kerbal)
		{
			if (Gender.IsFemale (kerbal)) {
				female_kerbals[kerbal.name] = new Female (kerbal);
			} else {
				male_kerbals[kerbal.name] = new Male (kerbal);
			}
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
			female_kerbals.Remove (kerbal.name);
		}

		public string name
		{
			get {
				return "progeny";
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (node.HasNode (name)) {
				var progeny = node.GetNode (name);
				if (Gender.IsFemale (kerbal)) {
					female_kerbals[kerbal.name] = new Female (kerbal, progeny);
				} else {
					male_kerbals[kerbal.name] = new Male (kerbal, progeny);
				}
			} else {
				AddKerbal (kerbal);
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			var progeny = new ConfigNode (name);
			node.AddNode (progeny);
			if (Gender.IsFemale (kerbal)) {
				female_kerbals[kerbal.name].Save (progeny);
			} else {
				male_kerbals[kerbal.name].Save (progeny);
			}
		}

		public void Clear ()
		{
			female_kerbals = new Dictionary<string, Female> ();
			male_kerbals = new Dictionary<string, Male> ();
			kerbal_vessels = new Dictionary<string, Vessel> ();
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			return null;
		}

		public ProgenyTracker ()
		{
			instance = this;
			Clear ();
			GameEvents.onKerbalStatusChange.Add (onKerbalStatusChange);
			GameEvents.onVesselCreate.Add (onVesselCreate);
		}

		~ProgenyTracker ()
		{
			instance = null;
			GameEvents.onKerbalStatusChange.Remove (onKerbalStatusChange);
			GameEvents.onVesselCreate.Remove (onVesselCreate);
		}

		void onKerbalStatusChange (ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
		{
		}

		internal IEnumerator<YieldInstruction> WaitAndGetCrew (Vessel vessel)
		{
			yield return null;
			var crew = vessel.GetVesselCrew ();
			for (int i = 0; i < crew.Count; i++) {
				Debug.Log(String.Format ("[KS Progeny] {0}", crew[i].name));
				kerbal_vessels[crew[i].name] = vessel;
			}
		}


		void onVesselCreate (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselCreate"));
			KSProgenyRunner.instance.StartCoroutine (WaitAndGetCrew (vessel));
		}

		string[] ShuffledNames (string[] names)
		{
			int len = names.Length;
			var kv = new List<KeyValuePair<float, string>> ();
			for (int i = 0; i < len; i++) {
				kv.Add (new KeyValuePair<float, string>(UnityEngine.Random.Range(0, 1f), names[i]));
			}
			var skv = (from item in kv orderby item.Key select item).ToArray ();
			string [] shuffled = new string[len];
			for (int i = 0; i < len; i++) {
				shuffled[i] = skv[i].Value;
			}
			return shuffled;
		}

		internal IEnumerator<YieldInstruction> ScanFemales ()
		{
			while (true) {
				//Debug.Log(String.Format ("[KS Progeny] ScanFemales"));
				string[] females = ShuffledNames (female_kerbals.Keys.ToArray ());
				yield return null;
				for (int i = 0; i < females.Length; i++) {
					if (!female_kerbals.ContainsKey (females[i])) {
						// the kerbal was removed so just skip to the next one
						continue;
					}
					//Debug.Log(String.Format ("[KS Progeny] ScanFemales: {0}", females[i]));
					female_kerbals[females[i]].Update ();
					yield return null;
				}
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class KSProgenyRunner : MonoBehaviour
	{
		internal static KSProgenyRunner instance;
		void Awake ()
		{
			if (!HighLogic.LoadedSceneIsGame || HighLogic.LoadedSceneIsEditor) {
				instance = null;
				return;
			}
			instance = this;
		}

		void OnDestroy ()
		{
			instance = null;
		}

		void Start ()
		{
			if (instance != null && ProgenyTracker.instance != null) {
				StartCoroutine (ProgenyTracker.instance.ScanFemales ());
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSProgenyInit : MonoBehaviour
	{
		void Awake ()
		{
			KerbalExt.AddModule (new ProgenyTracker ());
		}
	}
}
