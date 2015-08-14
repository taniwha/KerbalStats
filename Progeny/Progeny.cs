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
		Dictionary<string, Embryo> embryos;
		Dictionary<string, Juvenile> juveniles;
		Dictionary<string, Male> males;
		Dictionary<string, Female> females;
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

		public void Mature (Embryo embryo)
		{
			embryos.Remove (embryo.id);
			var juvenile = new Juvenile (embryo);
			juveniles[juvenile.id] = juvenile;
		}

		public void Mature (Juvenile juvenile)
		{
			juveniles.Remove (juvenile.id);
			if (juvenile.isFemale) {
				var female = new Female (juvenile);
				females[female.id] = female;
			} else {
				var male = new Male (juvenile);
				males[male.id] = male;
			}
		}

		public Embryo GetEmbryo (string id)
		{
			return embryos[id];
		}

		public void AddEmbryo (Embryo embryo)
		{
			embryos[embryo.id] = embryo;
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
			var embryo_list = config.GetNodes ("embryo");
			foreach (var z in embryo_list) {
				var embryo = new Embryo (z);
				embryos[embryo.id] = embryo;
			}
		}

		public override void OnSave (ConfigNode config)
		{
			ProgenySettings.Save (config);
			var id = bit_reverse (grey (zygote_id));
			config.AddValue ("zygote_id", id);
			foreach (var embryo in embryos.Values) {
				var node = config.AddNode ("embryo");
				embryo.Save (node);
			}
		}

		public override void OnAwake ()
		{
			embryos = new Dictionary<string, Embryo> ();
			juveniles = new Dictionary<string, Juvenile> ();
			males = new Dictionary<string, Male> ();
			females = new Dictionary<string, Female> ();
			enabled = false;
		}

		void OnDestroy ()
		{
		}
	}
}
