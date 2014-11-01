using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

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
			Debug.Log (String.Format ("[KS Exp] GetSeat: {0}", seat));
			return seat;
		}

		void SeatKerbal (ProtoCrewMember kerbal, Vessel vessel, string task)
		{
			double UT = Planetarium.GetUniversalTime ();
			ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			string situation = vessel.situation.ToString ();
			Debug.Log (String.Format ("[KS Exp] '{0}' '{1}' '{2}'",
									  kerbal.name, task, situation));
			ExperienceTracker.instance.BeginTask (kerbal, UT, task, situation);
		}

		IEnumerator<YieldInstruction> WaitAndSeatKerbal (ProtoCrewMember kerbal)
		{
			yield return null;
			Part part = kerbal.KerbalRef.InPart;
			Debug.Log (String.Format ("[KS Exp] WaitAndSeatKerbal: {0} {1} {2}",
									  kerbal.name, part, kerbal.seat));
			string pname = GetPartName (part);
			string seat = GetSeat (kerbal);
			string task = ExperienceTracker.partSeatTasks[pname][seat];
			SeatKerbal (kerbal, part.vessel, task);
		}

		void onCrewTransferred (GameEvents.HostedFromToAction<ProtoCrewMember,Part> hft)
		{
			var kerbal = hft.host;
			var src_part = hft.from;
			var dst_part = hft.to;
			Debug.Log (String.Format ("[KS Exp] onCrewTransferred: {0} {1} {2} '{3}'",
									  kerbal, src_part, dst_part, kerbal.seat));
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
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2} {3}",
									  "onKerbalStatusChange", kerbal.name, old_status, new_status));
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
					string pname = GetPartName (part);
					for (int j = 0; j < part.protoModuleCrew.Count; j++) {
						ProtoCrewMember kerbal = part.protoModuleCrew[j];
						Debug.Log (String.Format ("[KS Exp SVC l] {0} {1} {2}",
												  kerbal.name,
												  kerbal.seat, kerbal.seatIdx));
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
						Debug.Log (String.Format ("[KS Exp SVC ul] {0} {1} {2}",
												  kerbal.name,
												  seat, task));
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
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2}",
									  "onVesselCreate", vessel, vessel.protoVessel));
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

		void onVesselRecovered (ProtoVessel vessel)
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
			Debug.Log (String.Format ("[KS Exp] {0}: {1}",
									  "OnVesselRecoveryRequested", vessel));
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
			var oldsit = hft.from;
			var newsit = hft.to;
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2} {3}",
									  "onVesselSituationChange",
									  vessel.vesselName, oldsit, newsit));
			var crew = vessel.GetVesselCrew ();
			string situation = newsit.ToString ();
			double UT = Planetarium.GetUniversalTime ();
			foreach (var kerbal in crew) {
				ExperienceTracker.instance.SetSituation (kerbal, UT, situation);
			}
		}

		void onVesselSOIChanged (GameEvents.HostedFromToAction<Vessel, CelestialBody> hft)
		{
		}

		void Awake ()
		{
			var scene = HighLogic.LoadedScene;
			if (scene == GameScenes.SPACECENTER
				|| scene == GameScenes.EDITOR
				|| scene == GameScenes.FLIGHT
				|| scene == GameScenes.TRACKSTATION
				|| scene == GameScenes.SPH) {
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
