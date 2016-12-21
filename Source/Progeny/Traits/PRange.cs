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

	public class PRange
	{
		public double min { get; private set; }
		public double max { get; private set; }

		public PRange (double min, double max)
		{
			this.min = min;
			this.max = max;
		}

		public double P (double p)
		{
			return min * (1 - p) + max * p;
		}

		public double RevP (double p)
		{
			if (p <= min) {
				return 0;
			}
			if (p >= max) {
				return 1;
			}
			return (p - min) / (max - min);
		}
	}
}
