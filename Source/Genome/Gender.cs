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

	public class Gender : Trait
	{
		// The idea is 0 is Y, 1 is X, so zero bits means YY. For now, just
		// make that male and never generate it.
		// Indexed by bit count.
		static string[] genders = { "M", "M", "F" };

		public string name
		{
			get {
				return "Gender";
			}
		}

		public int GeneSize
		{
			get {
				return 1;
			}
		}

		public GenePair CreateGene (bool isFemale)
		{
			if (isFemale) {
				// female: XX
				return new GenePair (this, 1, 1);
			}
			// male: either XY or YX
			uint y = (uint) UnityEngine.Random.Range (0, 2);
			return new GenePair (this, y, 1 - y);
		}

		public GenePair CreateGene (string gender)
		{
			bool isFemale = gender[0] == 'f' || gender[0] == 'F';
			return CreateGene (isFemale);
		}

		public GenePair CreateGene (ProtoCrewMember pcm)
		{
			if (pcm == null) {
				var g = UnityEngine.Random.Range (0, 2);
				return CreateGene (g > 0);
			}
			return CreateGene (pcm.gender == ProtoCrewMember.Gender.Female);
		}

		public string CreateValue (GenePair gene)
		{
			uint index = (gene.a & 1) + (gene.b & 1);
			return genders[index];
		}
	}
}
