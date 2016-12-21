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

	public class BioClock
	{
		BioClockTC bc_trait;
		GenePair bioClockTC;
		GenePair bioClockInverse;
		double maturationK;
		PRange maturationP;
		double agingK;
		PRange agingP;

		public BioClock (GenePair[] genes)
		{
			for (int i = 0; i < genes.Length; i++) {
				var g = genes[i];
				switch (g.trait.name) {
					case "BioClockTC":
						bioClockTC = g;
						break;
					case "BioClockInverse":
						bioClockInverse = g;
						break;
					case "MaturationTimeK":
						maturationK = (g.trait as TimeK).K (g);
						break;
					case "MaturationTimeP":
						maturationP = (g.trait as TimeP).P (g);
						break;
					case "AgingTimeK":
						agingK = (g.trait as TimeK).K (g);
						break;
					case "AgingTimeP":
						agingP = (g.trait as TimeP).P (g);
						break;
				}
			}
			bc_trait = bioClockTC.trait as BioClockTC;
		}

		public double OvulationTime
		{
			get {
				return bc_trait.OvulationTime (bioClockTC, bioClockInverse);
			}
		}

		public double CyclePeriod
		{
			get {
				return bc_trait.CyclePeriod (bioClockTC, bioClockInverse);
			}
		}

		public double EggLife
		{
			get {
				return bc_trait.EggLife (bioClockTC, bioClockInverse);
			}
		}

		public double SpermLife
		{
			get {
				return bc_trait.SpermLife (bioClockTC, bioClockInverse);
			}
		}

		public double GestationPeriod
		{
			get {
				return bc_trait.GestationPeriod (bioClockTC, bioClockInverse);
			}
		}

		public double AgingTime (double p)
		{
			double lambda = bc_trait.AgingTime (bioClockTC, bioClockInverse);
			p = agingP.P (p);
			return MathUtil.WeibullQF (lambda, agingK, p);
		}

		public double MaturationTime (double p)
		{
			double lambda = bc_trait.MaturationTime (bioClockTC, bioClockInverse);
			p = maturationP.P (p);
			return MathUtil.WeibullQF (lambda, maturationK, p);
		}
	}
}
