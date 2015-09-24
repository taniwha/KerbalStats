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
using System.Collections.Generic;
using System.Linq;

namespace KerbalStats {
	public static class MathUtil {
		public static double WeibullCDF (double l, double k, double x)
		{
			return 1 - Math.Exp (-Math.Pow (x/l, k));
		}

		public static double WeibullQF (double l, double k, double p)
		{
			// t = l * (-ln(1-p)) ^ 1/k
			//ugh, why does .net not have log1p? Not that I expect the
			// random number generator to give that small a p
			return l * Math.Pow (-Math.Log (1 - p), 1/k);
		}

		public static float[] Binomial (double p, int n)
		{
			double q = 1 - p;
			double[] dist = new double[n + 1];
			float[] fdist = new float[n + 1];

			fdist[0] = (float) (dist[0] = Math.Pow (q, n));
			fdist[n] = (float) (dist[n] = Math.Pow (p, n));
			if (q == 0 || p == 0) {
				return fdist;
			}
			for (int i = 1; i < n; i++) {
				dist[i] = dist[i - 1] * (n + 1 - i) * p / (i * q);
				fdist[i] = (float) dist[i];
			}
			return fdist;
		}

	}
}
