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
using System.Linq;

namespace KerbalStats.Genome {

	public class GenePair
	{
		public Trait trait;
		public uint a;
		public uint b;

		public GenePair (Trait trait, uint a, uint b)
		{
			this.trait = trait;
			this.a = a;
			this.b = b;
		}

		public GenePair (GenePair gene)
		{
			trait = gene.trait;
			a = gene.a;
			b = gene.b;
		}

		public GenePair (Trait trait, string pair)
		{
			this.trait = trait;
			string[] genes = pair.Split (',');
			uint.TryParse (genes[0], out a);
			uint.TryParse (genes[1], out b);
		}

		public override string ToString ()
		{
			return a.ToString () + ", " + b.ToString ();
		}

		public static GenePair Combine (GenePair g1, GenePair g2)
		{
			uint a, b;
			Trait trait = g1.trait;

			if (UnityEngine.Random.Range (0, 2) != 0) {
				a = g1.a;
			} else {
				a = g1.b;
			}

			if (UnityEngine.Random.Range (0, 2) != 0) {
				b = g2.a;
			} else {
				b = g2.b;
			}

			if (UnityEngine.Random.Range (0, 2) != 0) {
				return new GenePair (trait, a, b);
			} else {
				return new GenePair (trait, b, a);
			}
		}
	}
}
