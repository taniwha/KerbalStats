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
	public class Juvenile : Zygote
	{
		double birthUT;
		double maturation;
		double subp;
		GenePair gender;
		GenePair maturationK;
		GenePair maturationP;

		public bool isFemale
		{
			get;
			private set;
		}

		void init ()
		{
			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "Gender":
						gender = genes[i];
						break;
					case "MaturationTimeK":
						maturationK = genes[i];
						break;
					case "MaturationTimeP":
						maturationP = genes[i];
						break;
				}
			}

			var g = gender.trait.CreateValue (gender);
			isFemale = (g == "F");

			var k = (maturationK.trait as TimeK).K (maturationK);
			var pRange = (maturationP.trait as TimeP).P (maturationP);
			var p = pRange.P (subp);
			BioClockTC bc_trait = bioClock.trait as BioClockTC;
			var l = bc_trait.MaturationTime (bioClock, bioClockInverse);
			maturation = MathUtil.WeibullQF(l, k, p);
		}

		public Juvenile (Embryo embryo) : base (embryo)
		{
			birthUT = embryo.Birth;
			init ();
		}

		public Juvenile (ConfigNode node) : base (node)
		{
			if (node.HasValue ("birthUT")) {
				double.TryParse (node.GetValue ("birthUT"), out birthUT);
			}
			if (node.HasValue ("p")) {
				double.TryParse (node.GetValue ("p"), out subp);
			} else {
				subp = UnityEngine.Random.Range (0, 1f);
			}
		}

		public override void Save (ConfigNode node)
		{
			node.AddValue ("birthUT", birthUT.ToString ("G17"));
			node.AddValue ("p", subp.ToString ("G17"));
		}

		public double Birth ()
		{
			return birthUT;
		}

		public double Maturation ()
		{
			return maturation;
		}
	}
}
