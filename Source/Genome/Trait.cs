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

	/** Interface for genetic traits
	 *
	 * Traits are defined by gene pairs (with a certain number of bits
	 * in each half of the pair). Interpretation of the bits in the gene
	 * pair is defined by the implementation, including the usefulness
	 * of random factors (eg, the gender trait would not use
	 * randomness).
	 *
	 * Because KSP generates kerbals randomly, any pre-generated kerbals
	 * need to have their genes "reverse engineered", so CreateGene() is
	 * used to create a gene pair that could produce the generated
	 * trait. For non-stock traits, the "reverse engineering" is
	 * optional and the gene can be freely generated.
	 */
	public interface Trait
	{
		/** Create a gene pair that can product the kerbal's trait.
		 */
		GenePair CreateGene (ProtoCrewMember kerbal, Random random);
		/** Generate the trait's value based on the gene and randomness.
		 */
		string CreateValue (GenePair gene, Random random);
		/** The name of the trait.
		 */
		string name { get; }

		/** The number of bits in a single gene of a gene pair.
		 */
		int GeneSize { get; }
	}
}
