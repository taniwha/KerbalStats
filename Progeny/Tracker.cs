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
		Dictionary <string, IKerbal> kerbals;

		Dictionary <Guid, List<Male>> boarded_males;
		Dictionary <string, Male> missing_males;
		Dictionary <string, Male> available_males;

		internal static ProgenyTracker instance;

		Dictionary <string, Vessel> kerbal_vessels;

		public void AddKerbal (ProtoCrewMember pcm)
		{
			IKerbal kerbal;
			if (Gender.IsFemale (pcm)) {
				kerbal = female_kerbals[pcm.name] = new Female (pcm);
			} else {
				kerbal = male_kerbals[pcm.name] = new Male (pcm);
			}
			kerbals[pcm.name] = kerbal;
		}

		public void RemoveKerbal (ProtoCrewMember pcm)
		{
			IKerbal kerbal = kerbals[pcm.name];
			kerbals.Remove (pcm.name);
			if (kerbal is Female) {
				female_kerbals.Remove (pcm.name);
			} else {
				male_kerbals.Remove (pcm.name);
			}
		}

		public string name
		{
			get {
				return "progeny";
			}
		}

		public void Load (ProtoCrewMember pcm, ConfigNode node)
		{
			if (node.HasNode (name)) {
				var progeny = node.GetNode (name);
				IKerbal kerbal;
				if (Gender.IsFemale (pcm)) {
					kerbal = female_kerbals[pcm.name] = new Female (pcm, progeny);
				} else {
					kerbal = male_kerbals[pcm.name] = new Male (pcm, progeny);
				}
				kerbals[pcm.name] = kerbal;
			} else {
				AddKerbal (pcm);
			}
		}

		public void Save (ProtoCrewMember pcm, ConfigNode node)
		{
			var progeny = new ConfigNode (name);
			node.AddNode (progeny);
			kerbals[pcm.name].Save (progeny);
		}

		public void Clear ()
		{
			female_kerbals = new Dictionary<string, Female> ();
			male_kerbals = new Dictionary<string, Male> ();
			kerbals = new Dictionary<string, IKerbal> ();

			kerbal_vessels = new Dictionary<string, Vessel> ();

			boarded_males = new Dictionary<Guid, List<Male>> ();
			missing_males = new Dictionary<string, Male> ();
			available_males = new Dictionary<string, Male> ();
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
			GameEvents.onCrewTransferred.Add (onCrewTransferred);
			GameEvents.onVesselCreate.Add (onVesselCreate);
			GameEvents.onVesselDestroy.Add (onVesselDestroy);
			GameEvents.onVesselWasModified.Add (onVesselWasModified);
		}

		~ProgenyTracker ()
		{
			instance = null;
			GameEvents.onKerbalStatusChange.Remove (onKerbalStatusChange);
			GameEvents.onCrewTransferred.Remove (onCrewTransferred);
			GameEvents.onVesselCreate.Remove (onVesselCreate);
			GameEvents.onVesselDestroy.Remove (onVesselDestroy);
			GameEvents.onVesselWasModified.Remove (onVesselWasModified);
		}

		internal IEnumerator<YieldInstruction> WaitAndCheckStatus (ProtoCrewMember pcm)
		{
			yield return null;
			if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available) {
				IKerbal kerbal = kerbals[pcm.name];
				Vessel v = kerbal_vessels[pcm.name];
				kerbal_vessels.Remove (pcm.name);
				if (kerbal is Male) {
					boarded_males[v.id].Remove (kerbal as Male);
					available_males[pcm.name] = kerbal as Male;
				}
			}
		}

		void onKerbalStatusChange (ProtoCrewMember pcm, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
		{
			if (newStatus == oldStatus) {
				// KSP doesn't check before firing the event.
				return;
			}
			IKerbal kerbal = kerbals[pcm.name];
			// Possible transitions (?):
			// Assigned->Available
			// Missing->Available
			// Available->Assigned
			// Missing->Dead
			// Assigned->Dead
			// Available->Dead (in theory. not in stock KSP)
			switch (newStatus) {
				case ProtoCrewMember.RosterStatus.Available:
					if (oldStatus == ProtoCrewMember.RosterStatus.Assigned) {
						// Status gets thrashed a little while transfering
						// (board, alight, transfer)
						// Check the kerbal's status again next frame. If it
						// is still Available, then the kerbal has been
						// recovered
						KSProgenyRunner.instance.StartCoroutine (WaitAndCheckStatus (pcm));
						return;
					}
					// Look what the cat dragged in.
					if (kerbal is Male) {
						missing_males.Remove (pcm.name);
						available_males[pcm.name] = kerbal as Male;
					}
					break;
				case ProtoCrewMember.RosterStatus.Assigned:
					// Let onCrewTransferred or onVesselCreate handle it.
					// Mostly because there is no information on where the
					// kerbal has been assigned.
					break;
				case ProtoCrewMember.RosterStatus.Missing:
					{
						Vessel v = kerbal_vessels[pcm.name];
						kerbal_vessels.Remove (pcm.name);
						if (kerbal is Male) {
							boarded_males[v.id].Remove (kerbal as Male);
							missing_males[pcm.name] = kerbal as Male;
						}
					}
					break;
				case ProtoCrewMember.RosterStatus.Dead:
					if (oldStatus == ProtoCrewMember.RosterStatus.Assigned) {
						Vessel v = kerbal_vessels[pcm.name];
						kerbal_vessels.Remove (pcm.name);
						if (kerbal is Male) {
							boarded_males[v.id].Remove (kerbal as Male);
						}
					} else if (oldStatus == ProtoCrewMember.RosterStatus.Missing) {
						if (kerbal is Male) {
							missing_males.Remove (pcm.name);
						}
					} else {
						if (kerbal is Male) {
							available_males.Remove (pcm.name);
						}
					}
					break;
			}
		}

		void onCrewTransferred (GameEvents.HostedFromToAction<ProtoCrewMember,Part> hft)
		{
		}

		internal IEnumerator<YieldInstruction> WaitAndGetCrew (Vessel vessel)
		{
			yield return null;
			var crew = vessel.GetVesselCrew ();
			var males = new List<Male> ();
			for (int i = 0; i < crew.Count; i++) {
				Debug.Log(String.Format ("[KS Progeny] {0}", crew[i].name));
				kerbal_vessels[crew[i].name] = vessel;
				IKerbal kerbal = kerbals[crew[i].name];
				if (kerbal is Male) {
					males.Add (kerbal as Male);
				}
			}
			boarded_males[vessel.id] = males;
		}

		void onVesselCreate (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselCreate"));
			KSProgenyRunner.instance.StartCoroutine (WaitAndGetCrew (vessel));
		}

		void onVesselDestroy (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselDestroy"));
			boarded_males.Remove (vessel.id);
		}

		void onVesselWasModified (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselWasModified"));
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
