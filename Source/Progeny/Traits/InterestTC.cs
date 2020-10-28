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

namespace KerbalStats.Progeny.Traits {
	using Genome;

	public class InterestTC : Trait
	{
		public string name { get { return "InterestTC"; } }
		public int GeneSize { get { return 4; } }

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

		public GenePair CreateGene (ProtoCrewMember pcm, Random random)
		{
			var gene = Genome.Prefab (this, pcm);
			if (gene == null) {
				gene = CreateGene (random);
			}
			return gene;
		}

		static double[] f = { 0.5, 1.0, 1.5, 2.0, 3.0 };

		public string CreateValue (GenePair gene, Random random)
		{
			var c = MathUtil.CountBits (gene.a & gene.b);
			double tc = ProgenySettings.InterestTC * f[c];
			return tc.ToString ();
		}
	}
}
