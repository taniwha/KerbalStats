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

		public string name
		{
			get {
				return kerbal.name;
			}
		}

		void initialize ()
		{
			aging = bioClock.AgingTime (subp);
		}

		public Adult (Juvenile juvenile) : base (juvenile)
		{
			birthUT = juvenile.Birth ();
			adulthoodUT = juvenile.Maturation ();
			kerbal = null;		// not yet recruited
			subp = UnityEngine.Random.Range (0, 1f);
			initialize ();
		}

		public Adult (ProtoCrewMember kerbal) : base (kerbal)
		{
			this.kerbal = kerbal;
			subp = UnityEngine.Random.Range (0, 1f);
			initialize ();
			CalcAdulthood ();
			CalcBirth ();
		}

		double YoungerP (double p)
		{
			p *= 0.999;	// avoid 1.0: bad juju (ln(0))
			// map 0..1 onto 0..inf via artanh, then feed that into
			// 1 - (x + 1) e^-x
			//
			// This remaps p to favor younger kerbals, but still allow older
			// ones.
			p = Math.Sqrt ((1 - p) / (1 + p));
			p = 1 - p * (1 - Math.Log (p));
			return p;
		}

		protected void CalcAdulthood ()
		{
			var UT = Planetarium.GetUniversalTime ();
			double p = UnityEngine.Random.Range (0, 1f);
			adulthoodUT = UT - aging * YoungerP (p);
		}

		// relies on adulthoodUT being known
		protected void CalcBirth ()
		{
			var p = UnityEngine.Random.Range (0, 1f);
			birthUT = adulthoodUT - bioClock.MaturationTime (p);
		}

		public Adult (ConfigNode node) : base (node)
		{
			this.kerbal = null;
			if (node.HasValue ("p")) {
				double.TryParse (node.GetValue ("p"), out subp);
			} else {
				subp = UnityEngine.Random.Range (0, 1f);
			}
			initialize ();
			if (node.HasValue ("adulthoodUT")) {
				double.TryParse (node.GetValue ("adulthoodUT"), out adulthoodUT);
			} else {
				CalcAdulthood ();
			}
			if (node.HasValue ("birthUT")) {
				double.TryParse (node.GetValue ("birthUT"), out birthUT);
			} else {
				CalcBirth ();
			}
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("birthUT", birthUT.ToString ("G17"));
			node.AddValue ("adulthoodUT", adulthoodUT.ToString ("G17"));
			node.AddValue ("p", subp.ToString ("G17"));

		}

		public double Birth ()
		{
			return birthUT;
		}

		public double Adulthood ()
		{
			return adulthoodUT;
		}

		public double Aging ()
		{
			return aging;
		}
	}
}
