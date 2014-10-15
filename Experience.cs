using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats {
	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class KSExperience : MonoBehaviour
	{
		class PartSeatTasks
		{
			public class SeatTasks
			{
				Dictionary <string, string> seats;
				string default_task;

				public string this [string seat]
				{
					get {
						if (seats.ContainsKey (seat)) {
							return seats[seat];
						} else {
							return default_task;
						}
					}
				}

				public SeatTasks (string defTask)
				{
					seats = new Dictionary <string, string> ();
					default_task = defTask;
				}
				public SeatTasks (ConfigNode node)
				{
					if (node.HasValue ("default")) {
						default_task = node.GetValue ("default");
					} else {
						default_task = "Passenger";
					}
					seats = new Dictionary <string, string> ();
					foreach (ConfigNode.Value seat in node.values) {
						if (seat.name == "name" || seat.name == "default") {
							continue;
						}
						seats[seat.name] = seat.value;
					}
				}
			}
			Dictionary <string, SeatTasks> partSeatTasks;
			SeatTasks default_seatTask;

			public SeatTasks this [string part]
			{
				get {
					if (partSeatTasks.ContainsKey (part)) {
						return partSeatTasks[part];
					} else {
						return default_seatTask;
					}
				}
			}

			public PartSeatTasks ()
			{
				var dbase = GameDatabase.Instance;
				default_seatTask = new SeatTasks ("Passenger");
				partSeatTasks = new Dictionary <string, SeatTasks> ();
				foreach (var seatMap in dbase.GetConfigNodes ("KSExpSeatMap")) {
					foreach (var partSeatMap in seatMap.GetNodes ("SeatTasks")) {
						string name = partSeatMap.GetValue ("name");
						if (name == null) {
							continue;
						}
						partSeatTasks[name] = new SeatTasks (partSeatMap);
					}
				}
			}
		}

		static PartSeatTasks partSeatTasks;

		void SetKerbalActivity (ProtoCrewMember kerbal, string task)
		{
			KerbalExt ext = KerbalStats.current[kerbal];
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2}",
									  "SetKerbalActivity", kerbal.name,
									  task));
			if (!ext.HasNode ("experience")) {
				ext.AddNode (new ConfigNode ("experience"));
			}
			var experience = ext.GetNode ("experience");
			var current = experience.GetValue ("_current");
			if (task != current) {
				double UT = Planetarium.GetUniversalTime ();
				if (current != null && current != "") {
					string start = experience.GetValue ("_currentUT");
					double start_time = double.Parse (start);
					double duration = UT - start_time;
					string exp = experience.GetValue (current);
					double expt = 0;
					if (exp != null && exp != "") {
						expt = double.Parse (exp);
					}
					expt += duration;
					exp = expt.ToString ("G17");
					if (!experience.SetValue (current, exp)) {
						experience.AddValue (current, exp);
					}
				}
				string taskUT = UT.ToString ("G17");
				if (!experience.SetValue ("_current", task)) {
					experience.AddValue ("_current", task);
				}
				if (!experience.SetValue ("_currentUT", taskUT)) {
					experience.AddValue ("_currentUT", taskUT);
				}
			}
		}

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
			SetKerbalActivity (kerbal, partSeatTasks[pname][seat]);
		}

		void onCrewOnEva (GameEvents.FromToAction<Part, Part> ft)
		{
			Part part = ft.from;
			ProtoCrewMember kerbal = ft.to.protoModuleCrew[0];
			Debug.Log (String.Format ("[KS Exp] {0}: {1} {2}",
									  "onCrewOnEva", part, kerbal.name));
			SetKerbalActivity (kerbal, "EVA");
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
			} else if (scene == GameScenes.MAINMENU) {
				if (partSeatTasks == null) {
					partSeatTasks = new PartSeatTasks ();
				}
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
