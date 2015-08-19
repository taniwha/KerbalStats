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

		public static List<Female> FemaleKerbals
		{
			get {
				return instance.female_kerbals.Values.ToList ();
			}
		}

		public static List<Male> MaleKerbals
		{
			get {
				return instance.male_kerbals.Values.ToList ();
			}
		}

		public static Vessel KerbalVessel (string name)
		{
			if (!instance.kerbal_vessels.ContainsKey (name)) {
				return null;
			}
			return instance.kerbal_vessels[name];
		}

		public static Vessel KerbalVessel (ProtoCrewMember pcm)
		{
			return KerbalVessel (pcm.name);
		}

		public static List<Male> BoardedMales (Vessel vessel)
		{
			return instance.boarded_males[vessel.id];
		}

		public static List<Male> AvailableMales ()
		{
			return instance.available_males.Values.ToList ();
		}

		void AddKerbal (IKerbal kerbal)
		{
			if (kerbal is Male) {
				switch (kerbal.kerbal.rosterStatus) {
					case ProtoCrewMember.RosterStatus.Available:
						available_males[kerbal.name] = kerbal as Male;
						break;
					case ProtoCrewMember.RosterStatus.Assigned:
						// vessel creation will take care of this
						break;
					case ProtoCrewMember.RosterStatus.Dead:
						// he's dead, Jim.
						break;
					case ProtoCrewMember.RosterStatus.Missing:
						missing_males[kerbal.name] = kerbal as Male;
						break;
				}
			}
			kerbals[kerbal.name] = kerbal;
		}

		public void AddKerbal (ProtoCrewMember pcm)
		{
			IKerbal kerbal;
			if (pcm.gender == ProtoCrewMember.Gender.Female) {
				kerbal = female_kerbals[pcm.name] = new Female (pcm);
			} else {
				kerbal = male_kerbals[pcm.name] = new Male (pcm);
			}
			ProgenyScenario.current.AddKerbal (kerbal);
			AddKerbal (kerbal);
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
			if (missing_males.ContainsKey (pcm.name)) {
				missing_males.Remove (pcm.name);
			}
			if (available_males.ContainsKey (pcm.name)) {
				available_males.Remove (pcm.name);
			}
		}

		public string name
		{
			get {
				return "progeny";
			}
		}

		IEnumerator WaitAndLoad (ProtoCrewMember pcm, ConfigNode node)
		{
			yield return null;
			if (node.HasValue (name)) {
				var id = node.GetValue (name);
				var kerbal = ProgenyScenario.current.GetKerbal (id);
				AddKerbal (kerbal);
			} else {
				AddKerbal (pcm);
			}
		}

		public void Load (ProtoCrewMember pcm, ConfigNode node)
		{
			ProgenyScenario.current.StartCoroutine (WaitAndLoad (pcm, node));
		}

		public void Save (ProtoCrewMember pcm, ConfigNode node)
		{
			var kerbal = kerbals[pcm.name];
			node.AddValue (name, kerbal.id);
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

		internal IEnumerator WaitAndCheckStatus (ProtoCrewMember pcm)
		{
			yield return null;
			if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available) {
				Debug.Log(String.Format ("[KS Progeny] WaitAndCheckStatus: {0} available", pcm.name));
				IKerbal kerbal = kerbals[pcm.name];
				Vessel v = kerbal_vessels[pcm.name];
				kerbal_vessels.Remove (pcm.name);
				if (kerbal is Male) {
					if (boarded_males.ContainsKey (v.id)) {
						boarded_males[v.id].Remove (kerbal as Male);
					} else {
						Debug.Log(String.Format ("[KS Progeny] WaitAndCheckStatus: no vessel {0}", v.id));
					}
					available_males[pcm.name] = kerbal as Male;
				}
			} else {
				Debug.Log(String.Format ("[KS Progeny] WaitAndCheckStatus: {0} {1}", pcm.name, pcm.rosterStatus));
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
						ProgenyScenario.current.StartCoroutine (WaitAndCheckStatus (pcm));
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
			if (hft.from != null && hft.to != null) {
				if (hft.from.vessel != hft.to.vessel) {
					Debug.Log(String.Format ("[KS Progeny] transfer: {0}", hft.host.name));
					kerbal_vessels[hft.host.name] = hft.to.vessel;
					IKerbal kerbal = kerbals[hft.host.name];
					if (kerbal is Male) {
						Vessel vf = hft.from.vessel;
						Vessel vt = hft.to.vessel;
						boarded_males[vf.id].Remove (kerbal as Male);
						// EVA spawns a new vessel, so onVesselCreate should
						// take care of things.
						if (boarded_males.ContainsKey (vt.id)) {
							boarded_males[vt.id].Add (kerbal as Male);
						}
					}
				}
			} else if (hft.from != null) {
				Debug.Log(String.Format ("[KS Progeny] transfer?1: {0}", hft.host.name));
			} else if (hft.to != null) {
				Debug.Log(String.Format ("[KS Progeny] transfer?2: {0}", hft.host.name));
			} else {
				Debug.Log(String.Format ("[KS Progeny] transfer?3: {0}", hft.host.name));
			}
		}

		internal IEnumerator WaitAndGetCrew (Vessel vessel)
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
			ProgenyScenario.current.StartCoroutine (WaitAndGetCrew (vessel));
		}

		void onVesselDestroy (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselDestroy"));
			boarded_males.Remove (vessel.id);
		}

		void onVesselWasModified (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselWasModified"));
			ProgenyScenario.current.StartCoroutine (WaitAndGetCrew (vessel));
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
