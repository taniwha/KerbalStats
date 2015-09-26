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

	public class Adult : Zygote, IKerbal
	{
		public ProtoCrewMember kerbal
		{
			get;
			set;
		}

		double birthUT;
		double adulthoodUT;
		double aging;
		double subp;
		GenePair agingK;
		GenePair agingP;

		public string name
		{
			get {
				return kerbal.name;
			}
		}

		void initialize ()
		{
			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "AgingTimeK":
						agingK = genes[i];
						break;
					case "AgingTimeP":
						agingP = genes[i];
						break;
				}
			}

			var k = (agingK.trait as AgingTimeK).K (agingK);
			var pRange = (agingP.trait as AgingTimeP).P (agingP);
			var p = pRange.P (subp);
			BioClock bc_trait = bioClock.trait as BioClock;
			var l = bc_trait.MaturationTime (bioClock, bioClockInverse);
			aging = MathUtil.WeibullQF(l, k, p);
		}

		public Adult (Juvenile juvenile) : base (juvenile)
		{
			birthUT = juvenile.Birth ();
			adulthoodUT = juvenile.Maturation ();
			kerbal = null;		// not yet recruited
			initialize ();
		}

		public Adult (ProtoCrewMember kerbal) : base (kerbal)
		{
			this.kerbal = kerbal;
			initialize ();
		}

		public Adult (ConfigNode node) : base (node)
		{
			this.kerbal = null;
			initialize ();
			if (node.HasValue ("birthUT")) {
				double.TryParse (node.GetValue ("birthUT"), out birthUT);
			}
			if (node.HasValue ("adulthoodUT")) {
				double.TryParse (node.GetValue ("adulthoodUT"), out adulthoodUT);
			}
			if (node.HasValue ("p")) {
				double.TryParse (node.GetValue ("p"), out subp);
			} else {
				subp = UnityEngine.Random.Range (0, 1f);
			}
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("birthUT", birthUT.ToString ("G17"));
			node.AddValue ("adulthoodUT", adulthoodUT.ToString ("G17"));
			node.AddValue ("p", subp.ToString ("G17"));

		}

		double Birth ()
		{
			return birthUT;
		}

		double Adulthood ()
		{
			return adulthoodUT;
		}

		double Aging ()
		{
			return aging;
		}
	}
}
