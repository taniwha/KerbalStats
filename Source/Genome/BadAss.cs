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

namespace KerbalStats.Genome {

	public class BadAss : Trait
	{
		/** Indexed by the number of 1 bits in the 6-bit genetic code (2
		 * 3-bit genes). The probability of getting badass for
		 * distributions 0 and 6 is arbitrarily set to 0 and 1
		 * respectively. For distributions 2, 3, and 4, the relative
		 * probability works out to 0.1 (distributions 0, 1, 5, and 6
		 * are not used for KSP generated kerbals). Distributions 1 and
		 * 5 are set to make the resulting curve feel reasonable.
		 *
		 * The distribution of the distributions follows the 6th-order
		 * binomial distribution (1 6 15 20 15 6 1). This comes
		 * naturally from counting the 1 bits in numbers from the range
		 * 0..63.
		 */
		static DiscreteDistribution[] distributions = {
			new DiscreteDistribution (new float[]{120f/120f,   0f/120f}),
			new DiscreteDistribution (new float[]{119f/120f,   1f/120f}),
			new DiscreteDistribution (new float[]{116f/120f,   4f/120f}),
			new DiscreteDistribution (new float[]{111f/120f,   9f/120f}),
			new DiscreteDistribution (new float[]{ 96f/120f,  24f/120f}),
			new DiscreteDistribution (new float[]{ 57f/120f,  63f/120f}),
			new DiscreteDistribution (new float[]{  0f/120f, 120f/120f}),
		};
		/** Select from distributions 2, 3, 4 (0, 1, 5, 6 not available
		 * to stock kerbals). This is used to determine the
		 * number of 1 bits in the 2x3-bit genetic code.
		 *
		 * Multiplying things out in the distributions table leads to
		 * the probability of a kerbal being badass as 1/100 for
		 * distribution 2, 3/100 for distribution 3, and 6/100 for
		 * distribution 3, or 29/100, 37/100, 24/100 respectively for a
		 * kerbal to not be badass.
		 */
		static DiscreteDistribution[] reverse = {
			// not badass
			new DiscreteDistribution (new float[]{29f/90f, 37f/90f, 24f/90f}),
			// is badass
			new DiscreteDistribution (new float[]{ 1f/10f,  3f/10f,  6f/10f}),
		};

		/** Value used for encoding the number of badass bits into the
		 * genome. Outer index is the number of desired bits, the inner
		 * is random.
		 */
		static uint[][] codes = {
			new uint[]{0, 0, 0},
			new uint[]{1, 2, 4},
			new uint[]{3, 5, 6},
			new uint[]{7, 7, 7},
		};

		public string name { get { return "BadAss"; } }

		public int GeneSize { get { return 3; } }

		/** Choose a distribution based on the gene pair.
		 */
		DiscreteDistribution ChooseDistribution (GenePair gene)
		{
			int index = MathUtil.CountBits (gene.a & 7) + MathUtil.CountBits (gene.b & 7);
			return distributions[index];
		}

		/** Create a random gene pair that fits the kerbal.
		 */
		public GenePair CreateGene (bool isBad, Random random)
		{
			DiscreteDistribution dist = reverse[isBad ? 1 : 0];
			// numBits will be 2, 3, or 4
			int numBits = 2 + dist.Value (random.Range (0, 1f));
			// max will be 3, 4 or 4,
			int max = numBits > 3 ? 4 : numBits + 1;
			// min will be 0, 0 or 1
			int min = numBits > 3 ? 1 : 0;
			// first will be 0, 1, 2, or 0, 1, 2, 3 or 1, 2, 3
			int first = random.Range (min, max);
			int a = random.Range (0, 3);
			int b = random.Range (0, 3);
			//Console.WriteLine(String.Format ("{0} {1} {2} {3} {4} {5}", numBits, min, max, first, a, b));
			return new GenePair (this, codes[first][a], codes[numBits - first][b]);
		}

		/** Create a random gene pair that fits the kerbal.
		 */
		public GenePair CreateGene (string isBadS, Random random)
		{
			bool isBad = false;
			bool.TryParse (isBadS, out isBad);
			return CreateGene (isBad, random);
		}

		/** Create a random gene pair that fits the kerbal.
		 */
		public GenePair CreateGene (ProtoCrewMember pcm, Random random)
		{
			if (pcm == null) {
				var b = random.Range (0, 2);
				return CreateGene (b > 0, random);
			}
			var gene = Genome.Prefab (this, pcm);
			if (gene == null) {
				gene = CreateGene (pcm.isBadass, random);
			}
			return gene;
		}

		/** Generate the badass value based on genes and randomness.
		 */
		public string CreateValue (GenePair gene, Random random)
		{
			DiscreteDistribution dist = ChooseDistribution (gene);
			return (dist.Value (random.Range (0, 1f)) > 0).ToString ();
		}
	}
}
