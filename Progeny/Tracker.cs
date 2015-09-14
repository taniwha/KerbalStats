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
		internal static ProgenyTracker instance;

		Dictionary<string, string> kerbal_ids;
		Dictionary<Guid, Vessel> vessels;

		Vessel vessel (Guid id)
		{
			if (!vessels.ContainsKey (id)) {
				return null;
			}
			return vessels[id];
		}

		public void AddKerbal (ProtoCrewMember pcm)
		{
			Zygote kerbal;
			if (pcm.gender == ProtoCrewMember.Gender.Female) {
				kerbal = new Female (pcm);
			} else {
				kerbal = new Male (pcm);
			}
			ProgenyScenario.current.AddKerbal (kerbal);
		}

		public void RemoveKerbal (ProtoCrewMember pcm)
		{
			kerbal_ids.Remove (pcm.name);
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
				kerbal_ids[pcm.name] = id;
			} else {
				AddKerbal (pcm);
			}
		}

		public void Load (ProtoCrewMember pcm, ConfigNode node)
		{
			KerbalStats.current.StartCoroutine (WaitAndLoad (pcm, node));
		}

		public void Save (ProtoCrewMember pcm, ConfigNode node)
		{
			node.AddValue (name, kerbal_ids[pcm.name]);
		}

		public void Clear ()
		{
			kerbal_ids = new Dictionary<string, string> ();
			vessels = new Dictionary<Guid, Vessel> ();
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			return null;
		}

		public ProgenyTracker (KerbalStats ks)
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
				var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[pcm.name]);
				var location = ProgenyScenario.current.GetLocation ("AstronautComplex");
				kerbal.SetLocation (location);

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
			// Possible transitions (?):
			// Assigned->Available
			// Missing->Available
			// Available->Assigned
			// Missing->Dead
			// Assigned->Dead
			// Available->Dead (in theory. not in stock KSP)
			var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[pcm.name]);
			Location location;
			switch (newStatus) {
				case ProtoCrewMember.RosterStatus.Available:
					if (oldStatus == ProtoCrewMember.RosterStatus.Assigned) {
						// Status gets thrashed a little while transfering
						// (board, alight, transfer)
						// Check the kerbal's status again next frame. If it
						// is still Available, then the kerbal has been
						// recovered
						KerbalStats.current.StartCoroutine (WaitAndCheckStatus (pcm));
						return;
					}
					// Look what the cat dragged in.
					break;
				case ProtoCrewMember.RosterStatus.Assigned:
					// Let onCrewTransferred or onVesselCreate handle it.
					// Mostly because there is no information on where the
					// kerbal has been assigned.
					break;
				case ProtoCrewMember.RosterStatus.Missing:
					location = ProgenyScenario.current.GetLocation ("Wilds");
					kerbal.SetLocation (location);
					break;
				case ProtoCrewMember.RosterStatus.Dead:
					location = ProgenyScenario.current.GetLocation ("Tomb");
					kerbal.SetLocation (location);
					break;
			}
		}

		void onCrewTransferred (GameEvents.HostedFromToAction<ProtoCrewMember,Part> hft)
		{
			var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[hft.host.name]);
			if (hft.from != null && hft.to != null) {
				if (hft.from.vessel != hft.to.vessel) {
					Debug.Log(String.Format ("[KS Progeny] transfer: {0}", hft.host.name));
					if (hft.to.vessel.isEVA) {
						// EVA spawns a new vessel, so onVesselCreate should
						// take care of things.
					} else {
						// boarded a vessel
						var location = ProgenyScenario.current.GetLocation ("Vessel", hft.to.vessel);
						kerbal.SetLocation (location);
					}
				} else {
					// transferes within a vessel have no effect
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
			var location = ProgenyScenario.current.GetLocation ("Vessel", vessel);
			var crew = vessel.GetVesselCrew ();
			for (int i = 0; i < crew.Count; i++) {
				Debug.Log(String.Format ("[KS Progeny] {0}", crew[i].name));
				var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[crew[i].name]);
				kerbal.SetLocation (location);
			}
		}

		void onVesselCreate (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselCreate"));
			KerbalStats.current.StartCoroutine (WaitAndGetCrew (vessel));
			Debug.Log(String.Format ("[KS Progeny] onVesselCreate a"));
			vessels[vessel.id] = vessel;
		}

		void onVesselDestroy (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselDestroy"));
			vessels.Remove (vessel.id);
		}

		void onVesselWasModified (Vessel vessel)
		{
			Debug.Log(String.Format ("[KS Progeny] onVesselWasModified"));
			KerbalStats.current.StartCoroutine (WaitAndGetCrew (vessel));
		}
	}
}
