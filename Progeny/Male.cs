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
	public class Male : IKerbal
	{
		public ProtoCrewMember kerbal
		{
			get;
			private set;
		}

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
			return (float) (1 - (x + 1) * Math.Exp (x));
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

		public Male (ProtoCrewMember kerbal)
		{
			this.kerbal = kerbal;
			interestTime = 0;
			interestTC = 3600;	//FIXME
		}

		public Male (ProtoCrewMember kerbal, ConfigNode progeny)
		{
			this.kerbal = kerbal;
			interestTime = 0;
			interestTC = 3600;  //FIXME
			if (progeny.HasValue ("interestTime")) {
				double.TryParse (progeny.GetValue ("interestTime"), out interestTime);
			}
			if (progeny.HasValue ("interestTC")) {
				double.TryParse (progeny.GetValue ("interestTC"), out interestTC);
			}
		}

		public void Save (ConfigNode progeny)
		{
			progeny.AddValue ("interestTime", interestTime.ToString ("G17"));
			progeny.AddValue ("interestTC", interestTC.ToString ("G17"));

		}

		public void UpdateStatus ()
		{
		}
	}
}
