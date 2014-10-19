using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Experience {
	public class ExperienceTracker : IKerbalStats
	{
		Dictionary<string,Experience> kerbal_experience;
		public static PartSeatTasks partSeatTasks;

		public static ExperienceTracker instance;

		public ExperienceTracker ()
		{
			instance = this;
			if (partSeatTasks == null) {
				partSeatTasks = new PartSeatTasks ();
			}
			kerbal_experience = new Dictionary<string,Experience> ();
		}

		public void AddKerbal (ProtoCrewMember kerbal)
		{
			kerbal_experience[kerbal.name] = new Experience ();
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
			kerbal_experience.Remove (kerbal.name);
		}

		public string name
		{
			get {
				return "experience";
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			kerbal_experience[kerbal.name] = new Experience ();
			kerbal_experience[kerbal.name].Load (node);
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (kerbal_experience.ContainsKey (kerbal.name)) {
				var exp = new ConfigNode ("experience");
				kerbal_experience[kerbal.name].Save (exp);
			}
		}

		public void SetSituation (ProtoCrewMember kerbal, double UT, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.SetSituation (UT, situation);
		}

		public void BeginTask (ProtoCrewMember kerbal, double UT, string task, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.BeginTask (UT, task, situation);
		}

		public void FinishTask (ProtoCrewMember kerbal, double UT, string task)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.FinishTask (UT, task);
		}

		public void FinishAllTasks (ProtoCrewMember kerbal, double UT)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			foreach (var task in exp.Current) {
				exp.FinishTask (UT, task);
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class KSExperienceTrackerEvents : MonoBehaviour
	{
		void onCrewBoardVessel (GameEvents.FromToAction<Part, Part> ft)
		{
			Part part = ft.to;
			// The "from" part is almost useless for getting the kerbal as
			// it has already been removed. Fortunately the part name is
			// formatted as "kerbalEVA (name)". The kerbal can then be found
			// in the part's crew list using the extracted name.
			string kname = ft.from.name;
			if (!kname.StartsWith ("kerbalEVA (")) {
				// don't know what to do with it
				return;
			}
			kname = kname.Substring (11, kname.Length - 12);
			ProtoCrewMember kerbal = null;
			foreach (var crew in part.protoModuleCrew) {
				if (crew.name == kname) {
					kerbal = crew;
					break;
				}
			}
			// Extract the actual part name from the part. Root nodes include
			// the vessel name :P
			string pname = part.name;
			if (pname.Contains (" (")) {
				pname = pname.Substring (0, pname.IndexOf (" ("));
			}
			// Try to find the seat name
			string seat = "";
			if (kerbal.seat != null) {
				seat = kerbal.seat.transform.name;
			}
			Debug.Log (String.Format ("[KS Exp] {0}: '{1}' '{2}' '{3}' {4} {5}",
									  "onCrewBoardVessel", kerbal.name,
									  pname, seat, kerbal.seat, kerbal.seatIdx));
			double UT = Planetarium.GetUniversalTime ();
			string task = ExperienceTracker.partSeatTasks[pname][seat];
			string situation = part.vessel.situation.ToString ();
			ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			ExperienceTracker.instance.BeginTask (kerbal, UT, task, situation);
		}

		void onCrewOnEva (GameEvents.FromToAction<Part, Part> ft)
		{
			Part part = ft.from;
			Vessel vessel = ft.to.vessel;
			ProtoCrewMember kerbal = ft.to.protoModuleCrew[0];
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2}",
									  "onCrewOnEva", part, kerbal.name));
			double UT = Planetarium.GetUniversalTime ();
			string situation = vessel.situation.ToString ();
			ExperienceTracker.instance.FinishAllTasks (kerbal, UT);
			ExperienceTracker.instance.BeginTask (kerbal, UT, "EVA", situation);
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
				GameEvents.onCrewBoardVessel.Add (onCrewBoardVessel);
				GameEvents.onCrewOnEva.Add (onCrewOnEva);
				GameEvents.onKerbalStatusChange.Add (onKerbalStatusChange);
				GameEvents.onKerbalTypeChange.Add (onKerbalTypeChange);
				GameEvents.onNewVesselCreated.Add (onNewVesselCreated);
				GameEvents.onPartCouple.Add (onPartCouple);
				GameEvents.onPartUndock.Add (onPartUndock);
				GameEvents.onVesselRecovered.Add (onVesselRecovered);
				GameEvents.onVesselSituationChange.Add (onVesselSituationChange);
				GameEvents.onVesselSOIChanged.Add (onVesselSOIChanged);
			} else if (scene == GameScenes.MAINMENU
					   && ExperienceTracker.instance == null) {
				KerbalStats.AddModule (new ExperienceTracker ());
			}
		}
		void OnDestroy ()
		{
			GameEvents.onCrewBoardVessel.Remove (onCrewBoardVessel);
			GameEvents.onCrewOnEva.Remove (onCrewOnEva);
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
