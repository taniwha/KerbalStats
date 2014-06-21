using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;

namespace ExLP {
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class ExToolbar_ShipInfo : MonoBehaviour
	{
//		private IButton button;

		public void Awake ()
		{
			//button = ToolbarManager.Instance.add ("KerbalStats", "button");
			//button.TexturePath = "KerbalStats/Textures/icon_button";
			//button.ToolTip = "EL Build Resources Display";
			//button.OnClick += (e) => ExShipInfo.ToggleGUI ();
		}

		void OnDestroy()
		{
			//button.Destroy ();
		}
	}
}
