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

using KerbalStats.Genome;

namespace KerbalStats.Progeny.Traits {

	public class TimeP
	{
		public int GeneSize { get { return 4; } }

		int CountBits (uint x)
		{
			uint count = 0;
			while (x > 0) {
				count += x & 1;
				x >>= 1;
			}
			return (int) count;
		}

		public GenePair CreateGene (Random random)
		{
			uint a = (uint) random.Range (0, 16);
			uint b = (uint) random.Range (0, 16);
			return new GenePair (this as Trait, a, b);
		}

		public GenePair CreateGene (string bio, Random random)
		{
			return CreateGene (random);
		}

		public string CreateValue (GenePair gene, Random random)
		{
			var c = gene.a & gene.b;
			return c.ToString ();
		}

		static PRange[] ranges = {
			new PRange (0, 0.7),
			new PRange (0.2, 0.5),
			new PRange (0.4, 0.8),
			new PRange (0.5, 0.9),
			new PRange (0.9, 0.99),
		};

		public virtual PRange P (GenePair gene)
		{
			var c = CountBits (gene.a & gene.b);
			return ranges[c];
		}
	}
}
