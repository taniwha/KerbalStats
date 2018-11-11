/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalStats.Progeny {
	using Locations;
	using Zygotes;

	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class DebugWindow : MonoBehaviour
	{
		static Rect winpos;
		static bool showGUI = false;
		static DebugWindow instance;

		enum InfoType {
			Embryos,
			Juveniles,
			Females,
			Males,
			Locations,
		};

		enum Locations {
			AstronautComplex,
			EVA,
			Tomb,
			Unknown,
			Wilds,
			Womb,
			Vessel,
		};

		InfoType infoType;
		Locations location;

		public static void ToggleGUI ()
		{
			showGUI = !showGUI;
			if (instance != null) {
				instance.enabled = showGUI;
			}
			if (!showGUI) {
				InputLockManager.RemoveControlLock ("KS_ProgenyDebug_window_lock");
			}
		}

		public static void LoadSettings (ConfigNode node)
		{
			string val = node.GetValue ("rect");
			if (val != null) {
				Quaternion pos;
				pos = ConfigNode.ParseQuaternion (val);
				winpos.x = pos.x;
				winpos.y = pos.y;
				winpos.width = pos.z;
				winpos.height = pos.w;
			}
			val = node.GetValue ("visible");
			if (val != null) {
				bool.TryParse (val, out showGUI);
			}
		}

		public static void SaveSettings (ConfigNode node)
		{
			Quaternion pos;
			pos.x = winpos.x;
			pos.y = winpos.y;
			pos.z = winpos.width;
			pos.w = winpos.height;
			node.AddValue ("rect", KSPUtil.WriteQuaternion (pos));
			node.AddValue ("visible", showGUI);
		}

		void Awake ()
		{
			instance = this;
			enabled = false;
		}

		void OnDestroy ()
		{
		}

		void OnGUI ()
		{
            if (!showGUI)
                return;

			if (winpos.x == 0 && winpos.y == 0) {
				winpos.x = Screen.width / 2;
				winpos.y = Screen.height / 2;
				winpos.width = 600;
				winpos.height = 100;
			}
			string ver = KSVersionReport.GetVersion ();
			winpos = GUILayout.Window (GetInstanceID (), winpos, debugWindow,
									  "Progeny Debug: " + ver,
									  GUILayout.MinWidth (200));
			if (enabled && winpos.Contains (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
				InputLockManager.SetControlLock ("KS_ProgenyDebug_window_lock");
			} else {
				InputLockManager.RemoveControlLock ("KS_ProgenyDebug_window_lock");
			}
		}

		void ZygoteName (Zygote zygote)
		{
			if (zygote is IKerbal && (zygote as IKerbal).kerbal != null) {
				GUILayout.Label ((zygote as IKerbal).kerbal.name);
			} else {
				GUILayout.Label ("null kerbal");
			}
		}

		void ShowEmbryos (IEnumerable<Embryo> embryos)
		{
			foreach (var e in embryos) {
				GUILayout.BeginHorizontal ();

				GUILayout.Label (e.id + ":");

				GUILayout.FlexibleSpace ();
				ZygoteName (ProgenyScenario.current.GetZygote(e.mother_id));
				GUILayout.FlexibleSpace ();
				ZygoteName (ProgenyScenario.current.GetZygote(e.father_id));
				GUILayout.FlexibleSpace ();
				double UT = Planetarium.GetUniversalTime ();
				double bUT = e.conceived + e.Birth;
				GUILayout.Label (((bUT - UT)/21600).ToString());
				GUILayout.EndHorizontal ();
			}
		}

		void ShowZygotes (IEnumerable<Zygote> zygotes)
		{
			foreach (var z in zygotes) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label (z.id + ":");
				GUILayout.FlexibleSpace ();
				if (z is IKerbal && (z as IKerbal).kerbal != null) {
					GUILayout.Label ((z as IKerbal).kerbal.name);
				} else {
					GUILayout.Label ("null kerbal");
				}
				if (z is Adult) {
					double UT = Planetarium.GetUniversalTime ();
					double age = UT - (z as Adult).Birth ();
					age /= KSPUtil.dateTimeFormatter.Year;
					GUILayout.Label (age.ToString ("F1"));
				}
				GUILayout.FlexibleSpace ();
				if (z.location != null) {
					GUILayout.Label (z.location.name);
				} else {
					GUILayout.Label ("null location");
				}
				GUILayout.EndHorizontal ();
				if (z is Female) {
					GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace ();
					GUILayout.Label ((z as Female).State);
					GUILayout.EndHorizontal ();
				}
			}
		}

		void LocationSelector ()
		{
			var location_list = EnumUtil.GetValues<Locations>();
			GUILayout.BeginHorizontal ();
			foreach (var t in location_list) {
				if (GUILayout.Toggle (location == t, t.ToString (),
									  GUILayout.Width (80))) {
					location = t;
				}
			}
			GUILayout.EndHorizontal ();
		}

		void ShowLocation (Location loc)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (loc.name + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (loc.isWatched ().ToString ());
			GUILayout.EndHorizontal ();
			foreach (var z in loc.Zygotes ()) {
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (z.id);
				GUILayout.EndHorizontal ();
			}
		}

		void ShowLocations ()
		{
			LocationSelector ();
			switch (location) {
				case Locations.AstronautComplex:
					ShowLocation (ProgenyScenario.current.locations.astronaut_complex);
					break;
				case Locations.EVA:
					ShowLocation (ProgenyScenario.current.locations.eva);
					break;
				case Locations.Tomb:
					ShowLocation (ProgenyScenario.current.locations.tomb);
					break;
				case Locations.Unknown:
					ShowLocation (ProgenyScenario.current.locations.unknown);
					break;
				case Locations.Wilds:
					ShowLocation (ProgenyScenario.current.locations.wilds);
					break;
				case Locations.Womb:
					ShowLocation (ProgenyScenario.current.locations.womb);
					break;
				case Locations.Vessel:
					foreach (var v in ProgenyScenario.current.locations.vessel_parts.Values) {
						ShowLocation (v);
					}
					break;
			}
		}

		void InfoSelector ()
		{
			var infotype_list = EnumUtil.GetValues<InfoType>();
			GUILayout.BeginHorizontal ();
			foreach (var t in infotype_list) {
				if (GUILayout.Toggle (infoType == t, t.ToString (),
									  GUILayout.Width (80))) {
					infoType = t;
				}
			}
			GUILayout.EndHorizontal ();
		}

		void debugWindow (int windowID)
		{
			if (ProgenyScenario.current == null) {
				return;
			}
			GUILayout.BeginVertical ();

			InfoSelector ();
			switch (infoType) {
				case InfoType.Embryos:
					ShowEmbryos (ProgenyScenario.current.Embryos);
					break;
				case InfoType.Juveniles:
					ShowZygotes (ProgenyScenario.current.Juveniles.Cast<Zygote>());
					break;
				case InfoType.Females:
					ShowZygotes (ProgenyScenario.current.Females.Cast<Zygote>());
					break;
				case InfoType.Males:
					ShowZygotes (ProgenyScenario.current.Males.Cast<Zygote>());
					break;
				case InfoType.Locations:
					ShowLocations ();
					break;
			}

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}
