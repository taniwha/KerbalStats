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
	public static class ProgenySettings
	{
		static bool settings_loaded;

		public static double AgingTime			{ get; private set; }
		public static double MaturationTime		{ get; private set; }
		public static double CyclePeriod		{ get; private set; }
		public static double GestationPeriod	{ get; private set; }
		public static double OvulationTime		{ get; private set; }
		public static double EggLife			{ get; private set; }
		public static double SpermLife			{ get; private set; }
		public static double InterestTC			{ get; private set; }

		public static void Load (ConfigNode config)
		{
			LoadGlobalSettings ();
			//Debug.Log (String.Format ("[KS:Progeny] Settings load"));
			var settings = config.GetNode ("Settings");
			if (settings == null) {
				settings = new ConfigNode ("Settings");
			//	gui_enabled = true; // Show settings window on first startup
			}

			//if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
			//	enabled = true;
			//}
		}

		public static void Save (ConfigNode config)
		{
			//Debug.Log (String.Format ("[KS:Progeny] Settings save: {0}", config));
			var settings = new ConfigNode ("Settings");
			config.AddNode (settings);
		}

		static void LoadGlobalSettings ()
		{
			if (settings_loaded) {
				return;
			}
			settings_loaded = true;

			// defining the kerbal "Julian" year to 426.09 21600s days makes
			// it about 1s short of Kerbin's orbital period
			AgingTime = 70 * 426.09 * 21600;	// a bit over 20.4 Earth years
			MaturationTime = 14 * 426.09 * 21600;	// a bit over 4 Earth years
			CyclePeriod = 56 * 21600;	// about one Minmus phase cycle
			GestationPeriod = 265 * 21600;	// close to a dog's
			OvulationTime = 0.5 * CyclePeriod;	// mid cycle
			EggLife = 3 * 21600;
			SpermLife = 3 * 3600;
			InterestTC = 3600;
			var dbase = GameDatabase.Instance;
			var settings = dbase.GetConfigNodes ("ProgenyGlobalSettings").LastOrDefault ();

			if (settings == null) {
				return;
			}
			double val;
			if (settings.HasValue ("AgingTime")) {
				if (double.TryParse (settings.GetValue ("AgingTime"), out val)) {
					AgingTime = val;
				}
			}
			if (settings.HasValue ("MaturationTime")) {
				if (double.TryParse (settings.GetValue ("MaturationTime"), out val)) {
					MaturationTime = val;
				}
			}
			if (settings.HasValue ("CyclePeriod")) {
				if (double.TryParse (settings.GetValue ("CyclePeriod"), out val)) {
					CyclePeriod = val;
				}
			}
			if (settings.HasValue ("GestationPeriod")) {
				if (double.TryParse (settings.GetValue ("GestationPeriod"), out val)) {
					GestationPeriod = val;
				}
			}
			if (settings.HasValue ("OvulationTime")) {
				if (double.TryParse (settings.GetValue ("OvulationTime"), out val)) {
					OvulationTime = val;
				}
			}
			if (settings.HasValue ("EggLife")) {
				if (double.TryParse (settings.GetValue ("EggLife"), out val)) {
					EggLife = val;
				}
			}
			if (settings.HasValue ("SpermLife")) {
				if (double.TryParse (settings.GetValue ("SpermLife"), out val)) {
					SpermLife = val;
				}
			}
			if (settings.HasValue ("InterestTC")) {
				if (double.TryParse (settings.GetValue ("InterestTC"), out val)) {
					InterestTC = val;
				}
			}
		}
	}
}
