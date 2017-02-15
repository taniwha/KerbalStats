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
	using Locations;
	using Zygotes;

	public class ProgenyTracker : IKerbalExt
	{
		internal static ProgenyTracker instance;

		Dictionary<string, string> kerbal_ids;
		Dictionary<Guid, Vessel> vessels;

		List<KerbalExt> loading_kerbals;
		bool reset_loading_kerbals;

		Vessel vessel (Guid id)
		{
			if (!vessels.ContainsKey (id)) {
				return null;
			}
			return vessels[id];
		}

		void onGameStateCreated (Game game)
		{
			Debug.LogFormat("[ProgenyTracker] onGameStateCreated: {0}", game.Title);
			reset_loading_kerbals = true;
			if (ProgenyScenario.current == null) {
				return;
			}
		}

		void onGameStatePostLoad (ConfigNode node)
		{
			Debug.LogFormat("[ProgenyTracker] onGameStatePostLoad: {0}", node);
		}

		void ProcessLoadingKerbals ()
		{
			Debug.Log("[ProgenyTracker] ProcessLoadingKerbals");
			if (loading_kerbals != null) {
				foreach (var ext in loading_kerbals) {
					if (kerbal_ids.ContainsKey (ext.kerbal.name)) {
						Debug.LogFormat("[ProgenyTracker] ProcessLoadingKerbals: already added {0}:{1}:{2}", ext.kerbal.name, ext[ModuleName], kerbal_ids[ext.kerbal.name]);
						ext[ModuleName] = kerbal_ids[ext.kerbal.name];
					} else {
						IKerbal kerbal = null;
						if (ext[ModuleName] != null) {
							kerbal = ProgenyScenario.current.GetKerbal (ext[ModuleName] as string) as IKerbal;
						}
						if (kerbal != null) {
							kerbal.kerbal = ext.kerbal;
							Debug.LogFormat("    {0} {1} {2}", ext.kerbal.name, ext[ModuleName], kerbal.id);
							kerbal_ids[ext.kerbal.name] = kerbal.id;
						} else {
							AddKerbal (ext);
						}
					}
				}
				loading_kerbals = null;
			}
		}

		void onProgenyScenarioLoaded (ProgenyScenario progeny)
		{
			ProcessLoadingKerbals ();
		}

		void AddLoadingKerbal(KerbalExt ext)
		{
			if (loading_kerbals == null || reset_loading_kerbals) {
				loading_kerbals = new List<KerbalExt> ();
				reset_loading_kerbals = false;
			}
			loading_kerbals.Add (ext);
		}

		public void AddKerbal (KerbalExt ext)
		{
			if (ProgenyScenario.current == null) {
				Debug.LogFormat("[ProgenyTracker] AddKerbal: delaying add {0}", ext.kerbal.name);
				AddLoadingKerbal (ext);
				return;
			}
			Debug.LogFormat("[ProgenyTracker] AddKerbal: adding kerbal");
			Zygote kerbal;
			if (ext.kerbal.gender == ProtoCrewMember.Gender.Female) {
				kerbal = new Female (ext.kerbal);
			} else {
				kerbal = new Male (ext.kerbal);
			}
			kerbal_ids[ext.kerbal.name] = kerbal.id;
			ProgenyScenario.current.AddKerbal (kerbal);
			ext[ModuleName] = kerbal.id;
			CheckLocation (ext.kerbal);
		}

		public void RemoveKerbal (KerbalExt kerbal)
		{
		}

		public string ModuleName
		{
			get {
				return "progeny";
			}
		}

		IEnumerator WaitAndAddKerbal (KerbalExt kerbal)
		{
			yield return null;
			yield return null;
			AddKerbal (kerbal);
		}

		public void Load (KerbalExt kerbal, ConfigNode node)
		{
			// Resuming a save can involve loading multiple saves before
			// loading the desired one, so ensure loading_kerbals is flushed
			// between loading passes.
			if (reset_loading_kerbals) {
				reset_loading_kerbals = false;
				loading_kerbals = null;
			}

			if (node.HasValue (ModuleName)) {
				var id = node.GetValue (ModuleName);
				kerbal[ModuleName] = id;
				AddLoadingKerbal (kerbal);
			} else {
				AddKerbal (kerbal);
				//if (!HighLogic.LoadedSceneIsEditor) {
				//	KerbalStats.current.StartCoroutine (WaitAndAddKerbal (kerbal));
				//}
			}
		}

		public void Save (KerbalExt kerbal, ConfigNode node)
		{
			// Ensure loading_kerbals gets flushed. The problem is that
			// onGameStateCreated is fired before the kerbal roster is created
			// for new games, and then not fired again until after loading
			// the new game once the space center has been entered. Load and
			// Save are never interleaved, so this is a good way of "detecting"
			// the end of the new game creation.
			reset_loading_kerbals = true;

			if (kerbal[ModuleName] == null) {
				// If the id is null, then the Progeny scenario never loaded
				// before saving. It is very likelly a new save was created.
				// The id will be set eventually when the scenario does load.
				Debug.LogFormat("[ProgenyTracker] not saving null id for {0}",
								kerbal.kerbal.name);
				return;
			}
			node.AddValue (ModuleName, kerbal[ModuleName] as string);
		}

		public void Clear ()
		{
			kerbal_ids = new Dictionary<string, string> ();
			vessels = new Dictionary<Guid, Vessel> ();
		}

		public string Get (KerbalExt kerbal, string parms)
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
			GameEvents.onGameStateCreated.Add (onGameStateCreated);
			GameEvents.onGameStatePostLoad.Add (onGameStatePostLoad);

			ProgenyScenario.onProgenyScenarioLoaded.Add (onProgenyScenarioLoaded);

			if (!HighLogic.LoadedSceneIsEditor) {
				KerbalStats.current.StartCoroutine (WaitAndCheckLocations ());
			}
		}

		public void Shutdown ()
		{
			instance = null;
			GameEvents.onKerbalStatusChange.Remove (onKerbalStatusChange);
			GameEvents.onCrewTransferred.Remove (onCrewTransferred);
			GameEvents.onVesselCreate.Remove (onVesselCreate);
			GameEvents.onVesselDestroy.Remove (onVesselDestroy);
			GameEvents.onVesselWasModified.Remove (onVesselWasModified);
			GameEvents.onGameStateCreated.Remove (onGameStateCreated);
			GameEvents.onGameStatePostLoad.Remove (onGameStatePostLoad);

			ProgenyScenario.onProgenyScenarioLoaded.Remove (onProgenyScenarioLoaded);
		}

		void CheckLocation (ProtoCrewMember kerbal)
		{
			var zygote = ProgenyScenario.current.GetKerbal (kerbal_ids[kerbal.name]);
			(zygote as IKerbal).kerbal = kerbal;
			Location location = null;
			switch (kerbal.rosterStatus) {
				case ProtoCrewMember.RosterStatus.Available:
					location = ProgenyScenario.current.GetLocation ("AstronautComplex");
					break;
				case ProtoCrewMember.RosterStatus.Assigned:
					//handled by the vessel scan, but fresh contract kerbals
					// (rescue or tourist) are Assigned but not on any vessel.
					// Avoid trampling on the vessel scan if it happens first,
					// but the scan will set the location if it happens second.
					if (zygote.location == null
						|| zygote.location is Unknown) {
						location = ProgenyScenario.current.GetLocation ("Unknown");
					}
					break;
				case ProtoCrewMember.RosterStatus.Missing:
					location = ProgenyScenario.current.GetLocation ("Wilds");
					break;
				case ProtoCrewMember.RosterStatus.Dead:
					location = ProgenyScenario.current.GetLocation ("Tomb");
					break;
			}
			if (location != null) {
				zygote.SetLocation (location);
			}
			Debug.LogFormat ("[ProgenyTracker] CheckLocation {0} '{1}'", kerbal.name, location);
		}

		internal IEnumerator WaitAndCheckLocations ()
		{
			while (ProgenyScenario.current == null) {
				yield return null;
			}
			yield return null;
			yield return null;
			Debug.LogFormat ("[ProgenyTracker] WaitAndCheckLocations");
			var game = HighLogic.CurrentGame;
			var roster = game.CrewRoster;
			for (int i = 0; i < roster.Count; i++) {
				CheckLocation (roster[i]);
			}
			for (int i = 0; i < FlightGlobals.Vessels.Count; i++) {
				GetCrew (FlightGlobals.Vessels[i]);
			}
		}

		internal IEnumerator WaitAndCheckStatus (ProtoCrewMember pcm)
		{
			yield return null;
			if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available) {
				Debug.LogFormat ("[ProgenyTracker] WaitAndCheckStatus: {0} available", pcm.name);
				var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[pcm.name]);
				var location = ProgenyScenario.current.GetLocation ("AstronautComplex");
				kerbal.SetLocation (location);

			} else {
				Debug.LogFormat ("[ProgenyTracker] WaitAndCheckStatus: {0} {1}", pcm.name, pcm.rosterStatus);
			}
		}

		void onKerbalStatusChange (ProtoCrewMember pcm, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
		{
			Debug.LogFormat ("[ProgenyTracker] onKerbalStatusChange: {0} {1} {2}", pcm.name, oldStatus, newStatus);
			if (string.IsNullOrEmpty (pcm.name)
				|| !kerbal_ids.ContainsKey (pcm.name)) {
				// premature event: the kerbal is still in the process of being
				// created by KSP
				return;
			}
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
					Debug.LogFormat ("[ProgenyTracker] onCrewTransferred: {0}", hft.host.name);
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
				Debug.LogFormat ("[ProgenyTracker] onCrewTransferred?1: {0}", hft.host.name);
			} else if (hft.to != null) {
				Debug.LogFormat ("[ProgenyTracker] onCrewTransferred?2: {0}", hft.host.name);
			} else {
				Debug.LogFormat ("[ProgenyTracker] onCrewTransferred?3: {0}", hft.host.name);
			}
		}

		internal void GetCrew (Vessel vessel)
		{
			var location = ProgenyScenario.current.GetLocation ("Vessel", vessel);
			var crew = vessel.GetVesselCrew ();
			for (int i = 0; i < crew.Count; i++) {
				Debug.LogFormat ("[ProgenyTracker] GetCrew {0} {1}", crew[i].name, location);
				var kerbal = ProgenyScenario.current.GetKerbal (kerbal_ids[crew[i].name]);
				kerbal.SetLocation (location);
			}
		}

		internal IEnumerator WaitAndGetCrew (Vessel vessel)
		{
			yield return null;
			if (vessel == null) {
				yield break;
			}
			GetCrew (vessel);
		}

		void onVesselCreate (Vessel vessel)
		{
			Debug.LogFormat ("[ProgenyTracker] onVesselCreate");
			KerbalStats.current.StartCoroutine (WaitAndGetCrew (vessel));
			Debug.LogFormat ("[ProgenyTracker] onVesselCreate a");
			vessels[vessel.id] = vessel;
		}

		void onVesselDestroy (Vessel vessel)
		{
			Debug.LogFormat ("[ProgenyTracker] onVesselDestroy");
			vessels.Remove (vessel.id);
		}

		void onVesselWasModified (Vessel vessel)
		{
			Debug.LogFormat ("[ProgenyTracker] onVesselWasModified");
			KerbalStats.current.StartCoroutine (WaitAndGetCrew (vessel));
		}
	}
}
