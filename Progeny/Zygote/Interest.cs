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

	public class Interest
	{
		GenePair InterestK;
		GenePair InterestTC;

		double interestTime;
		double interestTC;
		double interestK;

		public Interest (GenePair[] genes)
		{
			for (int i = 0; i < genes.Length; i++) {
				switch (genes[i].trait.name) {
					case "InterestK":
						InterestK = genes[i];
						break;
					case "InterestTC":
						InterestTC = genes[i];
						break;
				}
			}
			interestTime = 0;
			var iTC = InterestTC.trait.CreateValue (InterestTC);
			interestTC = double.Parse (iTC);
			var iK = InterestK.trait.CreateValue (InterestK);
			interestK = double.Parse (iK);
		}

		public void Load (ConfigNode node)
		{
			if (node.HasValue ("interestTime")) {
				double.TryParse (node.GetValue ("interestTime"), out interestTime);
			}
			if (node.HasValue ("interestTC")) {
				double.TryParse (node.GetValue ("interestTC"), out interestTC);
			}
			if (node.HasValue ("interestK")) {
				double.TryParse (node.GetValue ("interestK"), out interestK);
			}
		}

		public void Save (ConfigNode node)
		{
			node.AddValue ("interestTime", interestTime.ToString ("G17"));
			node.AddValue ("interestTC", interestTC.ToString ("G17"));
			node.AddValue ("interestK", interestK.ToString ("G17"));
		}

		public float isInterested (double UT)
		{
			if (UT < interestTime) {
				return 0;
			}
			double x = UT - interestTime;
			return (float) MathUtil.WeibulCDF (interestTC, interestK, x);
		}

		public void Mate (double UT)
		{
			interestTime = UT + 600; //FIXME
		}
	}
}
