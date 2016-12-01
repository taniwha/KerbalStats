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

	public class Male : Adult, IComparable<Male>
	{
		Interest interest;
		public Gamete gamete { get; private set; }

		public float isInterested (double UT)
		{
			return interest.isInterested (UT);
		}

		public double GameteLife ()
		{
			var p = UnityEngine.Random.Range (0, 1f);
			return gamete.Life (p);
		}

		public void Mate (double UT)
		{
			interest.Mate (UT);
		}

		void initialize ()
		{
			interest = new Interest (genes);
			gamete = new Gamete (genes, false, bioClock);
		}

		public Male (Juvenile juvenile) : base (juvenile)
		{
			initialize ();
		}

		public Male (ProtoCrewMember kerbal) : base (kerbal)
		{
			initialize ();
		}

		public Male (ConfigNode node) : base (node)
		{
			this.kerbal = null;
			initialize ();
			interest.Load (node);
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			interest.Save (node);
		}

		public int CompareTo (Male other)
		{
			return name.CompareTo (other.name);
		}
	}
}
