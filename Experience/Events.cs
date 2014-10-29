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
				seat = kerbal.seat.transform.name;
			}
			return seat;
		}

		void onCrewTransferred (GameEvents.HostedFromToAction<ProtoCrewMember,Part> hft)
		{
			var kerbal = hft.host;
			var src_part = hft.from;
			var dst_part = hft.to;
			Debug.Log (String.Format ("[KS Exp] onCrewTransferred: {0} {1} {2}",
									  kerbal, src_part, dst_part));
			double UT = Planetarium.GetUniversalTime ();
			ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			string task;
			Vessel vessel = dst_part.vessel;
			if (dst_part.vessel.isEVA) {
				task = "EVA";
			} else {
				string pname = GetPartName (dst_part);
				string seat = GetSeat (kerbal);
				task = ExperienceTracker.partSeatTasks[pname][seat];
			}
			string situation = vessel.situation.ToString ();
			Debug.Log (String.Format ("[KS Exp] '{0}' '{1}' '{2}'",
									  kerbal.name, task, situation));
			ExperienceTracker.instance.BeginTask (kerbal, UT, task, situation);
		}

		void onKerbalStatusChange (ProtoCrewMember pcm, ProtoCrewMember.RosterStatus old_status, ProtoCrewMember.RosterStatus new_status)
		{
			if (pcm.name == null || pcm.name == "") {
				// premature event: the kerbal is still in the process of being
				// created by KSP
				return;
			}
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2} {3}",
									  "onKerbalStatusChange", pcm.name, old_status, new_status));
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

		void onVesselRecovered (ProtoVessel vessel)
		{
			Debug.Log (String.Format ("[KS Exp] {0}: {1}",
									  "onVesselDestroy", vessel));
		}

		void onVesselSituationChange (GameEvents.HostedFromToAction<Vessel, Vessel.Situations> hft)
		{
			Vessel vessel = hft.host;
			var oldsit = hft.from;
			var newsit = hft.from;
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
				GameEvents.onVesselRecovered.Add (onVesselRecovered);
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
			GameEvents.onVesselRecovered.Remove (onVesselRecovered);
			GameEvents.onVesselSituationChange.Remove (onVesselSituationChange);
			GameEvents.onVesselSOIChanged.Remove (onVesselSOIChanged);
		}
	}
}
