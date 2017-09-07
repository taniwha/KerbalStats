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
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny.Zygotes {
	using Genome;
	using Traits;

	public class Gamete
	{
		double gameteK;
		PRange gameteP;
		double gameteL;

		public Gamete (Genome.Data data, bool isFemale, BioClock bioClock)
		{
			for (int i = 0; i < data.genes.Length; i++) {
				var g = data.genes[i];
				switch (g.trait.name) {
					case "GameteLifeK":
						gameteK = (g.trait as TimeK).K (g);
						break;
					case "GameteLifeP":
						gameteP = (g.trait as TimeP).P (g);
						break;
				}
			}
			if (isFemale) {
				gameteL = bioClock.EggLife;
			} else {
				gameteL = bioClock.SpermLife;
			}
		}

		public double Life (double p)
		{
			p = gameteP.P (p);
			return MathUtil.WeibullQF (gameteL, gameteK, p);
		}

		public float Viability (double time)
		{
			double p = MathUtil.WeibullCDF (gameteL, gameteK, time);
			p = gameteP.RevP (p);
			return (float) (1 - p);
		}
	}
}
