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

	public class BadAss : Trait
	{
		// Indexed by the number of 1 bits in the 6-bit genetic code (2 3-bit
		// genes). The probability of getting badass for distributions 0 and 6
		// is arbitrarily set to 0 and 1 respectively. For distributions 2, 3,
		// and 4, the relative probabily works out to 0.1 (distributions 0, 1,
		// 5, and 6 are not used for KSP generated kerbals). Distributions 1
		// and 5 are set to make the resulting curve feel reasonable.
		//
		// The distribution of the distributions follows the 6th-order binomial
		// distribution (1 6 15 20 15 6 1). This comes naturally from counting
		// the 1 bits in numbers from the range 0..63.
		static DiscreteDistribution[] distributions = {
			new DiscreteDistribution (new float[]{120f/120f,   0f/120f}),
			new DiscreteDistribution (new float[]{119f/120f,   1f/120f}),
			new DiscreteDistribution (new float[]{116f/120f,   4f/120f}),
			new DiscreteDistribution (new float[]{111f/120f,   9f/120f}),
			new DiscreteDistribution (new float[]{ 96f/120f,  24f/120f}),
			new DiscreteDistribution (new float[]{ 57f/120f,  63f/120f}),
			new DiscreteDistribution (new float[]{  0f/120f, 120f/120f}),
		};
		// Select from distributions 2, 3, 4 (0, 1, 5, 6 not available to stock
		// kerbals). However, this is used to determine the number of 1 bits in
		// the 2x3-bit genetic code.
		static DiscreteDistribution[] reverse = {
			new DiscreteDistribution (new float[]{29f/90f, 37f/90f, 24f/90f}),
			new DiscreteDistribution (new float[]{ 9f/90f, 27f/90f, 64f/90f}),
		};

		// Outer index is the number of desired bits, the inner is random
		static int[][] codes = {
			new int[]{0, 0, 0},
			new int[]{1, 2, 4},
			new int[]{3, 5, 6},
			new int[]{7, 7, 7},
		};

		public string name
		{
			get {
				return "BadAss";
			}
		}

		public int GeneSize
		{
			get {
				return 3;
			}
		}

		int CountBits (int x)
		{
			int count = 0;
			while (x > 0) {
				count += x & 1;
				x >>= 1;
			}
			return count;
		}

		DiscreteDistribution ChooseDistribution (GenePair gene)
		{
			int index = CountBits (gene.a & 7) + CountBits (gene.b & 7);
			return distributions[index];
		}

		public GenePair CreateGene (string isBadS)
		{
			bool isBad = false;
			bool.TryParse (isBadS, out isBad);
			DiscreteDistribution dist = reverse[isBad ? 1 : 0];
			int numBits = 2 + dist.Value (UnityEngine.Random.Range (0, 1f));
			int max = numBits > 3 ? 4 : numBits + 1;
			int min = numBits > 3 ? 1 : 0;
			int first = UnityEngine.Random.Range (min, max);
			int a = UnityEngine.Random.Range (0, 3);
			int b = UnityEngine.Random.Range (0, 3);
			//Console.WriteLine(String.Format ("{0} {1} {2} {3} {4} {5}", numBits, min, max, first, a, b));
			return new GenePair (codes[first][a], codes[numBits - first][b]);
		}

		public string CreateValue (GenePair gene)
		{
			DiscreteDistribution dist = ChooseDistribution (gene);
			return (dist.Value (UnityEngine.Random.Range (0, 1f)) > 0).ToString ();
		}
	}
}
