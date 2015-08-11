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
	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class DebugWindow : MonoBehaviour
	{
		static Rect winpos;
		static bool showGUI = false;
		static DebugWindow instance;

		enum InfoType {
			Females,
			Males,
			Vessels,
		};

		InfoType infoType;

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
				winpos.width = 300;
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

		void InfoSelector ()
		{
			GUILayout.BeginHorizontal ();
			for (var t = InfoType.Females; t <= InfoType.Vessels; t++) {
				if (GUILayout.Toggle (infoType == t, t.ToString (),
									  GUILayout.Width (80))) {
					infoType = t;
				}
			}
			GUILayout.EndHorizontal ();
		}

		void ShowKerbal (string name)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name + ":");
			GUILayout.FlexibleSpace ();
			var vessel = ProgenyTracker.KerbalVessel (name);
			if (vessel == null) {
				GUILayout.Label ("not boarded");
			} else {
				GUILayout.Label (vessel.vesselName);
			}
			GUILayout.EndHorizontal ();
		}

		void ShowKerbal (Male kerbal)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.Label (kerbal.name);
			GUILayout.EndHorizontal ();
		}

		void ShowVessel (Vessel vessel)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (vessel.vesselName);
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		void ListFemales ()
		{
			var females = ProgenyTracker.FemaleKerbals;
			females.Sort ();

			for (int i = 0; i < females.Count; i++) {
				ShowKerbal (females[i].name);
			}
		}

		void ListMales ()
		{
			var males = ProgenyTracker.MaleKerbals;
			males.Sort ();

			for (int i = 0; i < males.Count; i++) {
				ShowKerbal (males[i].name);
			}
		}

		void ListVessels ()
		{
			for (int i = 0; i < FlightGlobals.fetch.vessels.Count; i++) {
				var vessel = FlightGlobals.fetch.vessels[i];
				var males = ProgenyTracker.BoardedMales (vessel);
				if (males.Count < 1) {
					continue;
				}
				ShowVessel (vessel);
				for (int j = 0; j < males.Count; j++) {
					ShowKerbal (males[j]);
				}
			}
		}

		void debugWindow (int windowID)
		{
			GUILayout.BeginVertical ();

			InfoSelector ();
			switch (infoType) {
				case InfoType.Females:
					ListFemales ();
					break;
				case InfoType.Males:
					ListMales ();
					break;
				case InfoType.Vessels:
					ListVessels ();
					break;
			}

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}
