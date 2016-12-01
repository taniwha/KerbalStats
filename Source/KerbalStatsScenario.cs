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
using System.Linq;
using UnityEngine;

using Master = KerbalStats.KerbalStats;

namespace KerbalStats.Scenario {
	[KSPScenario(ScenarioCreationOptions.None,
				 GameScenes.SPACECENTER,
				 GameScenes.EDITOR,
				 GameScenes.FLIGHT,
				 GameScenes.TRACKSTATION)]
	public class KerbalStats : ScenarioModule
	{
		public override void OnLoad (ConfigNode config)
		{
			var game = HighLogic.CurrentGame;

			Debug.Log (String.Format ("[KS] OnLoad (scenario)"));
			var roster = config.GetNode ("Roster");

			KerbalExt.Clear ();

			if (roster != null) {
				var kerbal_list = roster.GetNodes ("KerbalExt");
				for (int i = 0; i < kerbal_list.Count(); i++) {
					var kerbal = kerbal_list[i];
					ProtoCrewMember pcm = game.CrewRoster[i];
					var ext = new KerbalExt ();
					ext.Load (pcm, kerbal);
					Master.current.SetExt (pcm, ext);
				}
			}
		}
	}

}
