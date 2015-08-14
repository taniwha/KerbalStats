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

namespace KerbalStats.Progeny {
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
			GameScenes.SPACECENTER,
			GameScenes.EDITOR,
			GameScenes.FLIGHT,
			GameScenes.TRACKSTATION,
		})
	]
	public class ProgenyScenario : ScenarioModule
	{
		Dictionary<string, Zygote> zygotes;
		uint zygote_id;

		public static uint bit_reverse (uint x)
		{
			uint y = 0;

			for (int i = 0; i < 32; i++) {
				y <<= 1;
				y |= x & 1;
				x >>= 1;
			}
			return y;
		}

		public static uint grey (uint x)
		{
			return x ^ (x >> 1);
		}

		public static uint rgrey (uint x)
		{
			for (var m = x; m != 0; m >>= 1) {
				x ^= m;
			}
			return x;
		}

		public Zygote GetZygote (string id)
		{
			return zygotes[id];
		}

		public void AddZygote (Zygote zygote)
		{
			zygotes[zygote.id] = zygote;
		}

		public string NextZygoteID ()
		{
			var id = bit_reverse (grey (++zygote_id));
			return id.ToString("x");
		}

		public static ProgenyScenario current
		{
			get {
				var game = HighLogic.CurrentGame;
				return game.scenarios.Select (s => s.moduleRef).OfType<ProgenyScenario> ().SingleOrDefault ();

			}
		}

		public override void OnLoad (ConfigNode config)
		{
			ProgenySettings.Load (config);
			var ids = config.GetValue ("zygote_id");
			uint id = 0;
			uint.TryParse (ids, out id);
			zygote_id = rgrey (bit_reverse (id));
			var zygote_list = config.GetNodes ("zygote");
			foreach (var z in zygote_list) {
				var zygote = new Zygote (z);
				zygotes[zygote.id] = zygote;
			}
		}

		public override void OnSave (ConfigNode config)
		{
			ProgenySettings.Save (config);
			var id = bit_reverse (grey (zygote_id));
			config.AddValue ("zygote_id", id);
			foreach (var zygote in zygotes.Values) {
				var node = config.AddNode ("zygote");
				zygote.Save (node);
			}
		}

		public override void OnAwake ()
		{
			zygotes = new Dictionary<string, Zygote> ();
			enabled = false;
		}

		void OnDestroy ()
		{
		}
	}
}
