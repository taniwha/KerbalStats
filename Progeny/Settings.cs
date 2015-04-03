/*
This file is part of KerbalStats:Progeny

KerbalStats:Progeny is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

KerbalStats:Progeny is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with KerbalStats:Progeny.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny {
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
			GameScenes.SPACECENTER,
			GameScenes.EDITOR,
			GameScenes.FLIGHT,
			GameScenes.TRACKSTATION,
		})
	]
	public class ProgenySettings : ScenarioModule
	{
		static bool settings_loaded;

		static string version = null;

		static Rect windowpos;
		private static bool gui_enabled;

		public static string GetVersion ()
		{
			if (version != null) {
				return version;
			}

			var asm = Assembly.GetCallingAssembly ();
			version =  KSVersionReport.GetAssemblyVersionString (asm);

			return version;
		}

		public static ProgenySettings current
		{
			get {
				var game = HighLogic.CurrentGame;
				return game.scenarios.Select (s => s.moduleRef).OfType<ProgenySettings> ().SingleOrDefault ();
			}
		}

		public override void OnLoad (ConfigNode config)
		{
			//Debug.Log (String.Format ("[KS:Progeny] Settings load"));
			//var settings = config.GetNode ("Settings");
			//if (settings == null) {
			//	settings = new ConfigNode ("Settings");
			//	gui_enabled = true; // Show settings window on first startup
			//}

			//if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
			//	enabled = true;
			//}
		}

		public override void OnSave(ConfigNode config)
		{
			//Debug.Log (String.Format ("[KS:Progeny] Settings save: {0}", config));
			//var settings = new ConfigNode ("Settings");
			//config.AddNode (settings);
		}

		void LoadGlobalSettings ()
		{
			if (settings_loaded) {
				return;
			}
			settings_loaded = true;

			CyclePeriod = 56 * 21600;	// about one Minmus phase cycle
			GestationPeriod = 265 * 21600;	// close to a dog's
			OvulationTime = 0.5;	// 0..1, phase of cycle
			EggLife = 3 * 21600;
			var dbase = GameDatabase.Instance;
			var settings = dbase.GetConfigNodes ("ProgenyGlobalSettings").LastOrDefault ();

			if (settings == null) {
				return;
			}
		}
		
		public override void OnAwake ()
		{
			LoadGlobalSettings ();

			enabled = false;
		}

		public static void ToggleGUI ()
		{
			gui_enabled = !gui_enabled;
		}

		void WindowGUI (int windowID)
		{
			GUILayout.BeginVertical ();

			if (GUILayout.Button ("OK")) {
				gui_enabled = false;
				InputLockManager.RemoveControlLock ("KS:Progeny_Settings_window_lock");
			}
			GUILayout.EndVertical ();
			GUI.DragWindow (new Rect (0, 0, 10000, 20));
		}

		void OnGUI ()
		{
			if (enabled) { // don't do any work at all unless we're enabled
				if (gui_enabled) { // don't create windows unless we're going to show them
					GUI.skin = HighLogic.Skin;
					if (windowpos.x == 0) {
						windowpos = new Rect (Screen.width / 2 - 250,
							Screen.height / 2 - 30, 0, 0);
					}
					string name = "KerbalStats:Progeny";
					string ver = GetVersion ();
					windowpos = GUILayout.Window (GetInstanceID (),
						windowpos, WindowGUI,
						name + " " + ver,
						GUILayout.Width (500));
					if (windowpos.Contains (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
						InputLockManager.SetControlLock ("KS:Progeny_Settings_window_lock");
					} else {
						InputLockManager.RemoveControlLock ("KS:Progeny_Settings_window_lock");
					}
				} else {
					InputLockManager.RemoveControlLock ("KS:Progeny_Settings_window_lock");
				}
			}
		}
	}
}
