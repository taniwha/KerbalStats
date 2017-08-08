/*
This file is part of KerbalStats.

KerbalStats is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

KerbalStats is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with KerbalStats.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.UI.Screens;

namespace KerbalStats {
	using Toolbar;

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSAppButton : MonoBehaviour
	{
		const ApplicationLauncher.AppScenes buttonScenes = ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH;
		private static ApplicationLauncherButton button = null;

		public static Callback Toggle = delegate {};

		static bool buttonVisible
		{
			get {
				//if (ToolbarManager.Instance == null) {
				//	return true;
				//}
				//return false;
				return true;
			}
		}

		public static void UpdateVisibility ()
		{
			if (button != null) {
				button.VisibleInScenes = buttonVisible ? buttonScenes : 0;
			}
		}

		void onToggle ()
		{
			//Toggle ();
			Progeny.DebugWindow.ToggleGUI ();
		}

		void Start ()
		{
			GameObject.DontDestroyOnLoad(this);
			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
		}

		void OnDestroy ()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
		}

		void OnGUIAppLauncherReady ()
		{
			if (ApplicationLauncher.Ready && button == null) {
				var tex = GameDatabase.Instance.GetTexture("KerbalStats/Textures/progeny_icon_button", false);
				button = ApplicationLauncher.Instance.AddModApplication(onToggle, onToggle, null, null, null, null, buttonScenes, tex);
				UpdateVisibility ();
			}
		}
	}

	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class KSToolbar_ProgenyDebug : MonoBehaviour
	{
		private IButton button;

		public void Awake ()
		{
			if (ToolbarManager.Instance == null) {
				return;
			}
			button = ToolbarManager.Instance.add ("KerbalStats", "KS_P_Debug");
			button.TexturePath = "KerbalStats/Textures/progeny_icon_button";
			button.ToolTip = "KerbalStats Progeny Debug";
			button.OnClick += (e) => Progeny.DebugWindow.ToggleGUI ();
		}

		void OnDestroy()
		{
			if (button != null) {
				button.Destroy ();
			}
		}
	}
}
