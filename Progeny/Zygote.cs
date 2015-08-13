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
	public class Zygote
	{
		Female mother;
		Male father;
		GenePair[] genes;
		double conceived;
		double birth;
		double l;
		double k;
		double p;

		public Zygote (Female mother, Male father)
		{
			this.mother = mother;
			this.father = father;
			genes = Genome.Combine (mother.kerbal, father.kerbal);
			l = ProgenySettings.GestationPeriod;
			k = 10;//FIXME make genetic
			p = UnityEngine.Random.Range (0, 1f);
			birth = CalcBirth ();
			conceived = Planetarium.GetUniversalTime ();
		}

		public Zygote (ConfigNode node)
		{
			string name;
			name = node.GetValue ("mother");
			mother = ProgenyTracker.instance[name] as Female;
			name = node.GetValue ("father");
			father = ProgenyTracker.instance[name] as Male;
			genes = Genome.ReadGenes (node);
			if (node.HasValue ("conceived")) {
				double.TryParse (node.GetValue ("conceived"), out conceived);
			}
			if (node.HasValue ("l")) {
				double.TryParse (node.GetValue ("l"), out l);
			} else {
				l = ProgenySettings.GestationPeriod;
			}
			if (node.HasValue ("k")) {
				double.TryParse (node.GetValue ("k"), out k);
			} else {
				k = 10;//FIXME make genetic
			}
			if (node.HasValue ("p")) {
				double.TryParse (node.GetValue ("p"), out p);
			} else {
				p = UnityEngine.Random.Range (0, 1f);
			}
			birth = CalcBirth ();
		}

		double CalcBirth ()
		{
			// t = l * (-ln(1-p)) ^ 1/k
			//ugh, why does .net not have log1p? Not that I expect the
			// random number generator to give that small a p
			return l * Math.Pow (-Math.Log (1 - p), 1/k);
		}

		public void Save (ConfigNode node)
		{
			node.AddValue ("l", l.ToString ("G17"));
			node.AddValue ("k", k.ToString ("G17"));
			node.AddValue ("p", p.ToString ("G17"));
			node.AddValue ("conceived", conceived.ToString ("G17"));
			node.AddValue ("mother", mother.name);
			node.AddValue ("father", father.name);
			Genome.WriteGenes (genes, node);
		}

		public double Birth
		{
			get {
				return birth;
			}
		}
	}
}
