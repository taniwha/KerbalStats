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
	public class Embryo: Zygote
	{
		// As a simplification, this coverers all stages from conception to
		// birth (zygote, proembryo, embryo, fetus).
		double conceived;
		double birth;
		double subp;

		public Embryo (Female mother, Male father) : base (mother, father)
		{
			subp = UnityEngine.Random.Range (0, 1f);
			conceived = Planetarium.GetUniversalTime ();
			birth = CalcBirth ();
			SetLocation (ProgenyScenario.current.GetLocation ("Womb"));
		}

		public Embryo (ConfigNode node) : base (node)
		{
			if (node.HasValue ("conceived")) {
				double.TryParse (node.GetValue ("conceived"), out conceived);
			}
			if (node.HasValue ("p")) {
				double.TryParse (node.GetValue ("p"), out subp);
			} else {
				subp = UnityEngine.Random.Range (0, 1f);
			}
			birth = CalcBirth ();
		}

		double CalcBirth ()
		{
			double k = 1;
			PRange pRange = null;

			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "GestationPeriodK":
						k = (genes[i].trait as TimeK).K (genes[i]);
						break;
					case "GestationPeriodP":
						pRange = (genes[i].trait as TimeP).P (genes[i]);
						break;
				}
			}
			var p = pRange.P (subp);
			BioClock bc_trait = bioClock.trait as BioClock;
			var l = bc_trait.GestationPeriod (bioClock, bioClockInverse);
			return MathUtil.WeibullQF(l, k, p);
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("p", subp.ToString ("G17"));
			node.AddValue ("conceived", conceived.ToString ("G17"));
		}

		public double Birth
		{
			get {
				return birth;
			}
		}
	}
}
