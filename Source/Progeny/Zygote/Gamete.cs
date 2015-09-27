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

namespace KerbalStats.Progeny {
	using Genome;

	public class Gamete
	{
		GenePair gameteK;
		GenePair gameteP;
		double gameteL;
		PRange pRange;

		public Gamete (GenePair[] genes, bool isFemale, Zygote zygote)
		{
			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "GameteLifeK":
						gameteK = genes[i];
						break;
					case "GameteLifeP":
						gameteP = genes[i];
						break;
				}
			}
			GenePair bioClock = zygote.bioClock;
			GenePair bioClockInverse = zygote.bioClockInverse;
			BioClockTC bc_trait = bioClock.trait as BioClockTC;
			pRange = (gameteP.trait as TimeP).P (gameteP);
			if (isFemale) {
				gameteL = bc_trait.EggLife (bioClock, bioClockInverse);
			} else {
				gameteL = bc_trait.SpermLife (bioClock, bioClockInverse);
			}
		}

		public double Life (double p)
		{
			var k = (gameteK.trait as TimeK).K (gameteK);
			p = pRange.P (p);
			return MathUtil.WeibullQF (gameteL, k, p);
		}

		public float Viability (double time)
		{
			var k = (gameteK.trait as TimeK).K (gameteK);
			double p = MathUtil.WeibullCDF (gameteL, k, time);
			p = pRange.RevP (p);
			return (float) (1 - p);
		}
	}
}
