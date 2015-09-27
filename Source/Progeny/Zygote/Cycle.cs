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

	public class Cycle
	{
		GenePair cycleK;
		GenePair cycleP;
		PRange cyclePR;
		double cycleL;

		GenePair ovulationK;
		GenePair ovulationP;
		PRange ovulationPR;
		double ovulationL;

		double cycle_start;
		double cycle_end;
		double ovulation_time;

		public Cycle (GenePair[] genes, Zygote zygote)
		{
			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "CyclePeriodK":
						cycleK = genes[i];
						break;
					case "CyclePeriodP":
						cycleP = genes[i];
						break;
					case "OvulationTimeK":
						ovulationK = genes[i];
						break;
					case "OvulationTimeP":
						ovulationP = genes[i];
						break;
				}
			}
			GenePair bioClock = zygote.bioClock;
			GenePair bioClockInverse = zygote.bioClockInverse;
			BioClockTC bc_trait = bioClock.trait as BioClockTC;

			cycleL = bc_trait.CyclePeriod (bioClock, bioClockInverse);
			ovulationL = bc_trait.OvulationTime (bioClock, bioClockInverse);

			cyclePR = (cycleP.trait as TimeP).P (cycleP);
			ovulationPR = (ovulationP.trait as TimeP).P (ovulationP);
		}

		double CalcCyclePeriod (double p)
		{
			var k = (cycleK.trait as TimeK).K (cycleK);
			p = cyclePR.P (p);
			return MathUtil.WeibullQF (cycleL, k, p);
		}

		double CalcOvulationTime (double p)
		{
			var k = (ovulationK.trait as TimeK).K (ovulationK);
			p = ovulationPR.P (p);
			return MathUtil.WeibullQF (ovulationL, k, p);
		}

		public void Update (double UT)
		{
			float p;

			while (cycle_end < UT) {
				// hopefully not too many cycles have gone between the last
				// update and now
				p = UnityEngine.Random.Range (0, 1f);
				cycle_start = cycle_end;
				cycle_end = cycle_start + CalcCyclePeriod (p);
			}
			p = UnityEngine.Random.Range (0, 1f);
			ovulation_time = cycle_start + CalcOvulationTime (p);
		}

		public double OvulationTime { get { return ovulation_time; } }

		public float NonmatingFactor (double UT)
		{
			// the lower this is, the more likely the female is to mate
			// put the peak of interest (or trough of rejection) a little
			// before ovulation
			var x = 1.75 * (UT - cycle_start) / (ovulation_time - cycle_start);
			return (float) (1 - x * x * Math.Exp (-x));
		}

		public void Load (ConfigNode node)
		{
			if (node.HasValue ("cycle_start")) {
				double.TryParse (node.GetValue ("cycle_start"), out cycle_start);
			}
			if (node.HasValue ("cycle_end")) {
				double.TryParse (node.GetValue ("cycle_end"), out cycle_end);
			}
			if (node.HasValue ("ovulation_time")) {
				double.TryParse (node.GetValue ("ovulation_time"), out ovulation_time);
			}
		}

		public void Save (ConfigNode node)
		{
			node.AddValue ("cycle_start", cycle_start.ToString ("G17"));
			node.AddValue ("cycle_end", cycle_end.ToString ("G17"));
			node.AddValue ("ovulation_time", ovulation_time.ToString ("G17"));
		}
	}
}
