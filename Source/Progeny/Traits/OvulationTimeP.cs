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

namespace KerbalStats.Progeny {

	public class OvulationTimeP : TimeP, Trait
	{
		public string name { get { return "OvulationTimeP"; } }

		public GenePair CreateGene (ProtoCrewMember pcm)
		{
			return CreateGene ();
		}
	}
}
