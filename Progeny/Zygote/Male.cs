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

	public class Male : Zygote, IKerbal, IComparable<Male>
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

		double interestTime;
		double interestTC;

		public string name
		{
			get {
				return kerbal.name;
			}
		}

		public float Interest (double UT)
		{
			if (UT < interestTime) {
				return 0;
			}
			double x = (UT - interestTime) / interestTC;
			return (float) (1 - (x + 1) * Math.Exp (-x));
		}

		public float Fertility
		{
			get {
				return 0.95f;//FIXME
			}
		}

		public void Mate (double interestTime)
		{
			this.interestTime = interestTime;
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
			// t = l * (-ln(1-p)) ^ 1/k
			//ugh, why does .net not have log1p? Not that I expect the
			// random number generator to give that small a p
			aging = l * Math.Pow (-Math.Log (1 - p), 1/k);


			interestTime = 0;
			interestTC = 3600;	//FIXME
		}

		public Male (Juvenile juvenile) : base (juvenile)
		{
			birthUT = juvenile.Birth ();
			adulthoodUT = juvenile.Maturation ();
			kerbal = null;		// not yet recruited
			initialize ();
		}

		public Male (ProtoCrewMember kerbal) : base (kerbal)
		{
			this.kerbal = kerbal;
			initialize ();
		}

		public Male (ConfigNode node) : base (node)
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
			if (node.HasValue ("interestTime")) {
				double.TryParse (node.GetValue ("interestTime"), out interestTime);
			}
			if (node.HasValue ("interestTC")) {
				double.TryParse (node.GetValue ("interestTC"), out interestTC);
			}
		}

		public override void Save (ConfigNode node)
		{
			Debug.Log(String.Format ("[KS Male] Save: '{0}' '{1}' '{2}'", kerbal.name, interestTime, interestTC));
			base.Save (node);
			node.AddValue ("birthUT", birthUT.ToString ("G17"));
			node.AddValue ("adulthoodUT", adulthoodUT.ToString ("G17"));
			node.AddValue ("p", subp.ToString ("G17"));
			node.AddValue ("interestTime", interestTime.ToString ("G17"));
			node.AddValue ("interestTC", interestTC.ToString ("G17"));

		}

		public int CompareTo (Male other)
		{
			return name.CompareTo (other.name);
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
