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

	/** Holds the actual genetic data representing a trait
	 */
	public class GenePair
	{
		/** The trait represented by this gene pair
		 */
		public Trait trait;
		public uint a;		///< one of the genes in the pair
		public uint b;		///< one of the genes in the pair

		/** Create a gene pair from a trait and two gene encodings
		 */
		public GenePair (Trait trait, uint a, uint b)
		{
			this.trait = trait;
			this.a = a;
			this.b = b;
		}

		/** Copy constructor
		 */
		public GenePair (GenePair gene)
		{
			trait = gene.trait;
			a = gene.a;
			b = gene.b;
		}

		/** Construct a gne pair from a trait and pair string
		 *
		 * Used for loading.
		 */
		public GenePair (Trait trait, string pair)
		{
			this.trait = trait;
			string[] genes = pair.Split (',');
			uint.TryParse (genes[0], out a);
			uint.TryParse (genes[1], out b);
		}

		/** Represent the genetic data as a string
		 *
		 * Used for saving.
		 */
		public override string ToString ()
		{
			return a.ToString () + ", " + b.ToString ();
		}

		/** Produce a new gene pair from genetic recombintation of two pairs
		 *
		 * \todo implement crossover for genes longer than 1 bit
		 */
		public static GenePair Combine (GenePair g1, GenePair g2, Random random)
		{
			uint a, b;
			Trait trait = g1.trait;

			if (random.Range (0, 2) != 0) {
				a = g1.a;
			} else {
				a = g1.b;
			}

			if (random.Range (0, 2) != 0) {
				b = g2.a;
			} else {
				b = g2.b;
			}

			if (random.Range (0, 2) != 0) {
				return new GenePair (trait, a, b);
			} else {
				return new GenePair (trait, b, a);
			}
		}
	}
}
