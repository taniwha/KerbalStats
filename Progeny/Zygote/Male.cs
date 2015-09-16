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
	public class Male : Zygote, IKerbal, IComparable<Male>
	{
		public ProtoCrewMember kerbal
		{
			get;
			set;
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
			interestTime = 0;
			interestTC = 3600;	//FIXME
		}

		public Male (Juvenile juvenile) : base (juvenile)
		{
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
			node.AddValue ("interestTime", interestTime.ToString ("G17"));
			node.AddValue ("interestTC", interestTC.ToString ("G17"));

		}

		public int CompareTo (Male other)
		{
			return name.CompareTo (other.name);
		}
	}
}
