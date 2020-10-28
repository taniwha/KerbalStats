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

	public class Cycle
	{
		Genome.Data data;
		double cycleK;
		PRange cycleP;
		double cycleL;

		double ovulationK;
		PRange ovulationP;
		double ovulationL;

		double recuperationK;
		PRange recuperationP;
		double recuperationL;

		double cycle_start;
		double cycle_end;
		double ovulation_time;

		public Cycle (Genome.Data data, BioClock bioClock)
		{
			this.data = data;
			for (int i = 0; i < data.genes.Length; i++) {
				var g = data.genes[i];
				switch (g.trait.name) {
					case "CyclePeriodK":
						cycleK = (g.trait as TimeK).K (g);
						break;
					case "CyclePeriodP":
						cycleP = (g.trait as TimeP).P (g);
						break;
					case "OvulationTimeK":
						ovulationK = (g.trait as TimeK).K (g);
						break;
					case "OvulationTimeP":
						ovulationP = (g.trait as TimeP).P (g);
						break;
					case "RecuperationTimeK":
						recuperationK = (g.trait as TimeK).K (g);
						break;
					case "RecuperationTimeP":
						recuperationP = (g.trait as TimeP).P (g);
						break;
				}
			}

			cycleL = bioClock.CyclePeriod;
			ovulationL = bioClock.OvulationTime;
			recuperationL = bioClock.RecuperationTime;
		}

		double CalcCyclePeriod (double p)
		{
			p = cycleP.P (p);
			return MathUtil.WeibullQF (cycleL, cycleK, p);
		}

		double CalcOvulationTime (double p)
		{
			p = ovulationP.P (p);
			return MathUtil.WeibullQF (ovulationL, ovulationK, p);
		}

		double CalcRecuperationTime (double p)
		{
			p = recuperationP.P (p);
			return MathUtil.WeibullQF (recuperationL, recuperationK, p);
		}

		public void Update (double UT)
		{
			float p;

			while (cycle_end < UT) {
				// hopefully not too many cycles have gone between the last
				// update and now
				p = data.random.Range (0, 1f);
				cycle_start = cycle_end;
				cycle_end = cycle_start + CalcCyclePeriod (p);
			}
			if (ovulation_time < cycle_start) {
				p = data.random.Range (0, 1f);
				ovulation_time = cycle_start + CalcOvulationTime (p);
			}
		}

		public void Recuperate (double UT)
		{
			float p;
			p = data.random.Range (0, 1f);
			cycle_start = UT + CalcRecuperationTime (p);
			p = data.random.Range (0, 1f);
			cycle_end = cycle_start + CalcCyclePeriod (p);
			p = data.random.Range (0, 1f);
			ovulation_time = cycle_start + CalcOvulationTime (p);
		}

		public bool Recuperating (double UT)
		{
			return UT < cycle_start;
		}

		public double OvulationTime { get { return ovulation_time; } }

		public float NonmatingFactor (double UT)
		{
			// the lower this is, the more likely the female is to mate
			// put the peak of interest (or trough of rejection) at
			// ovulation
			var x = 2 * (UT - cycle_start) / (ovulation_time - cycle_start);
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
