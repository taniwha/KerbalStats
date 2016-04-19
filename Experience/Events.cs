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
using KSP.UI.Screens;

namespace KerbalStats.Experience {
	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class KSExperienceTrackerEvents : MonoBehaviour
	{
		string GetPartName (Part part)
		{
			// Extract the actual part name from the part. Root nodes include
			// the vessel name :P
			string pname = part.name;
			if (pname.Contains (" (")) {
				pname = pname.Substring (0, pname.IndexOf (" ("));
			}
			return pname;
		}

		string GetSeat (ProtoCrewMember kerbal)
		{
			// Try to find the seat name
			string seat = "";
			if (kerbal.seat != null) {
				seat = kerbal.seat.seatTransformName;
			}
			return seat;
		}

		void SeatKerbal (ProtoCrewMember kerbal, Vessel vessel, string task)
		{
			double UT = Planetarium.GetUniversalTime ();
			ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			string situation = vessel.situation.ToString ();
			string body = vessel.mainBody.bodyName;
			ExperienceTracker.instance.BeginTask (kerbal, UT, task, body,
												  situation);
		}

		IEnumerator<YieldInstruction> WaitAndSeatKerbal (ProtoCrewMember kerbal)
		{
			yield return null;
			Part part = kerbal.KerbalRef.InPart;
			string pname = GetPartName (part);
			string seat = GetSeat (kerbal);
			string task = ExperienceTracker.partSeatTasks[pname][seat];
			SeatKerbal (kerbal, part.vessel, task);
		}

		void onCrewTransferred (GameEvents.HostedFromToAction<ProtoCrewMember,Part> hft)
		{
			var kerbal = hft.host;
			var dst_part = hft.to;
			if (dst_part.vessel.isEVA) {
				Vessel vessel = dst_part.vessel;
				SeatKerbal (kerbal, vessel, "EVA");
			} else {
				StartCoroutine (WaitAndSeatKerbal (kerbal));
			}
		}

		void onKerbalStatusChange (ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus old_status, ProtoCrewMember.RosterStatus new_status)
		{
			if (kerbal.name == null || kerbal.name == "") {
				// premature event: the kerbal is still in the process of being
				// created by KSP
				return;
			}
			if (new_status == ProtoCrewMember.RosterStatus.Dead) {
				double UT = Planetarium.GetUniversalTime ();
				ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			}
		}

		void onKerbalTypeChange (ProtoCrewMember pcm, ProtoCrewMember.KerbalType old_type, ProtoCrewMember.KerbalType new_type)
		{
			if (pcm.name == null || pcm.name == "") {
				// premature event: the kerbal is still in the process of being
				// created by KSP
				return;
			}
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2} {3}",
									  "onKerbalTypeChange", pcm.name, old_type, new_type));
		}

		void onNewVesselCreated (Vessel vessel)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1}",
									  "onNewVesselCreated", vessel));
		}

		void onPartCouple (GameEvents.FromToAction<Part, Part> ft)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2}",
									  "onPartCouple", ft.from, ft.to));
		}

		void onPartUndock (Part p)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1}",
									  "onPartUndock", p));
		}

		InternalModel GetInternal (ProtoPartSnapshot ppart)
		{
			InternalModel ip = null;
			if (ppart.partInfo.internalConfig != null) {
				var iname = ppart.partInfo.internalConfig.GetValue ("name");
				if (iname != null && iname != "") {
					ip = PartLoader.GetInternalPart (iname);
				}
			}
			return ip;
		}

		void ScanVesselCrew (Vessel vessel)
		{
			if (vessel.loaded) {
				for (int i = 0; i < vessel.parts.Count; i++) {
					Part part = vessel.parts[i];
					if (part == null || part.name == null) {
						// the part has been deleted. Probably EL loading
						// a craft for cost checking.
						return;
					}
					string pname = GetPartName (part);
					for (int j = 0; j < part.protoModuleCrew.Count; j++) {
						ProtoCrewMember kerbal = part.protoModuleCrew[j];
						string seat = GetSeat (kerbal);
						string task = ExperienceTracker.partSeatTasks[pname][seat];
						SeatKerbal (kerbal, vessel, task);
					}
				}
			} else {
				ProtoVessel pv = vessel.protoVessel;
				for (int i = 0; i < pv.protoPartSnapshots.Count; i++) {
					ProtoPartSnapshot pp = pv.protoPartSnapshots[i];
					var ip = GetInternal (pp);
					for (int j = 0; j < pp.protoModuleCrew.Count; j++) {
						ProtoCrewMember kerbal = pp.protoModuleCrew[j];
						string seat = "";
						if (ip != null && ip.seats != null &&
							kerbal.seatIdx < ip.seats.Count) {
							seat = ip.seats[kerbal.seatIdx].seatTransformName;
						}
						string pname = pp.partName;
						string task = ExperienceTracker.partSeatTasks[pname][seat];
						SeatKerbal (kerbal, vessel, task);
					}
				}
			}
		}

		IEnumerator<YieldInstruction> WaitAndScanVesselCrew (Vessel vessel)
		{
			// Newly created vessels for launch are populated with assigned
			// kerbals in the same frame as the vessel is created, but after
			// the onVesselCreate event has fired, so wait one frame before
			// scanning for crew.
			yield return null;
			if (vessel.isEVA) {
				// Kerbals going EVA are handled by onCrewTransferred()
				yield break;
			}
			ScanVesselCrew (vessel);
		}

		void onVesselCreate (Vessel vessel)
		{
			if (vessel.protoVessel == null) {
				// This is a newly created vessel.
				// Crew have yet to be assigned to their positions.
				StartCoroutine (WaitAndScanVesselCrew (vessel));
			} else {
				// This is an existing vessel loaded from a saved game.
				// Any crew are already on board.
				// However, the KerbalStats scenario is loaded after vessels,
				// so wait a frame for the scenario to load.
				StartCoroutine (WaitAndScanVesselCrew (vessel));
			}
		}

		void onVesselRecovered (ProtoVessel vessel, bool quick)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1}",
									  "onVesselRecovered", vessel));
		}

		void onVesselRecoveryProcessing (ProtoVessel vessel, MissionRecoveryDialog d, float f)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2} {3}",
									  "onVesselRecoveryProcessing", vessel, d, f));
		}

		void OnVesselRecoveryRequested (Vessel vessel)
		{
			double UT = Planetarium.GetUniversalTime ();
			if (vessel.loaded) {
				for (int i = 0; i < vessel.parts.Count; i++) {
					Part part = vessel.parts[i];
					for (int j = 0; j < part.protoModuleCrew.Count; j++) {
						ProtoCrewMember kerbal = part.protoModuleCrew[j];
						ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
					}
				}
			} else {
				ProtoVessel pv = vessel.protoVessel;
				for (int i = 0; i < pv.protoPartSnapshots.Count; i++) {
					ProtoPartSnapshot pp = pv.protoPartSnapshots[i];
					for (int j = 0; j < pp.protoModuleCrew.Count; j++) {
						ProtoCrewMember kerbal = pp.protoModuleCrew[j];
						ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
					}
				}
			}
		}

		void onVesselSituationChange (GameEvents.HostedFromToAction<Vessel, Vessel.Situations> hft)
		{
			Vessel vessel = hft.host;
			var newsit = hft.to;
			var crew = vessel.GetVesselCrew ();
			string situation = newsit.ToString ();
			string body = vessel.mainBody.bodyName;
			double UT = Planetarium.GetUniversalTime ();
			foreach (var kerbal in crew) {
				ExperienceTracker.instance.SetSituation (kerbal, UT, body,
														 situation);
			}
		}

		IEnumerator<YieldInstruction> WaitAndSetBody (Vessel vessel)
		{
			yield return null;
			yield return null;
			var crew = vessel.GetVesselCrew ();
			string situation = vessel.situation.ToString ();
			string body = vessel.mainBody.bodyName;
			double UT = Planetarium.GetUniversalTime ();
			foreach (var kerbal in crew) {
				ExperienceTracker.instance.SetSituation (kerbal, UT, body,
														 situation);
			}
		}

		void onVesselSOIChanged (GameEvents.HostedFromToAction<Vessel, CelestialBody> hft)
		{
			Vessel vessel = hft.host;
			StartCoroutine (WaitAndSetBody (vessel));
		}

		void Awake ()
		{
			var scene = HighLogic.LoadedScene;
			if (scene == GameScenes.SPACECENTER
				|| scene == GameScenes.EDITOR
				|| scene == GameScenes.FLIGHT
				|| scene == GameScenes.TRACKSTATION) {
				GameEvents.onCrewTransferred.Add (onCrewTransferred);
				GameEvents.onKerbalStatusChange.Add (onKerbalStatusChange);
				GameEvents.onKerbalTypeChange.Add (onKerbalTypeChange);
				GameEvents.onNewVesselCreated.Add (onNewVesselCreated);
				GameEvents.onPartCouple.Add (onPartCouple);
				GameEvents.onPartUndock.Add (onPartUndock);
				GameEvents.onVesselCreate.Add (onVesselCreate);
				GameEvents.onVesselRecovered.Add (onVesselRecovered);
				GameEvents.onVesselRecoveryProcessing.Add (onVesselRecoveryProcessing);
				GameEvents.OnVesselRecoveryRequested.Add (OnVesselRecoveryRequested);
				GameEvents.onVesselSituationChange.Add (onVesselSituationChange);
				GameEvents.onVesselSOIChanged.Add (onVesselSOIChanged);
			}
		}
		void OnDestroy ()
		{
			GameEvents.onCrewTransferred.Remove (onCrewTransferred);
			GameEvents.onKerbalStatusChange.Remove (onKerbalStatusChange);
			GameEvents.onKerbalTypeChange.Remove (onKerbalTypeChange);
			GameEvents.onNewVesselCreated.Remove (onNewVesselCreated);
			GameEvents.onPartCouple.Remove (onPartCouple);
			GameEvents.onPartUndock.Remove (onPartUndock);
			GameEvents.onVesselCreate.Remove (onVesselCreate);
			GameEvents.onVesselRecovered.Remove (onVesselRecovered);
			GameEvents.onVesselRecoveryProcessing.Remove (onVesselRecoveryProcessing);
			GameEvents.OnVesselRecoveryRequested.Remove (OnVesselRecoveryRequested);
			GameEvents.onVesselSituationChange.Remove (onVesselSituationChange);
			GameEvents.onVesselSOIChanged.Remove (onVesselSOIChanged);
		}
	}
}
