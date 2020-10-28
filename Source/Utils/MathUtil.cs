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
		/** Implement the Weibull cumulative distribution function
		 *
		 * \param l     scale parameter (lambda)
		 * \param k     shape parameter
		 * \param x     time to event
		 * \return      the probability of the event occuring before time x
		 */
		public static double WeibullCDF (double l, double k, double x)
		{
			return 1 - Math.Exp (-Math.Pow (x/l, k));
		}

		/** Implement log(1 + x) because .net doesn't expose it.
		 *
		 * Taken from stackexchange, but modified to be a little tighter on
		 * small x: testing in python showed that log(1+x) is still more
		 * accurate than the simple Taylor series at x=1e-5.
		 */
		public static double Log1p (double x)
		{
			return Math.Abs (x) > 1e-5 ? Math.Log (1 + x) : (-0.5 * x + 1) * x;
		}

		/** Implement the Weibull quantile function
		 *
		 * ie, the inverse of the WeibullCDF()
		 *
		 * \param l     scale parameter (lambda)
		 * \param k     shape parameter
		 * \param p     the probability of the event having occured
		 * \return      the expected time until the event for the given
		 *              probability
		 */
		public static double WeibullQF (double l, double k, double p)
		{
			// t = l * (-ln(1-p)) ^ 1/k
			return l * Math.Pow (Log1p (-p), 1/k);
			//ugh, why does .net not have log1p? Not that I expect the
			// random number generator to give that small a p
			//return l * Math.Pow (-Math.Log (1 - p), 1/k);
		}

		/** Generate a binomial distribution
		 *
		 * Produces the full probability array for 0-n events.
		 *
		 * \param p     The probablility of a single event
		 * \param n     The maximum number of possible events
		 * \return      The array of n+1 probabilities for 0-n events.
		 *
		 * \note        If n is too large (somewhere around 15), the
		 *              the combinatorials will lose precision.
		 * \bug         should probably use double instead of float
		 */
		public static float[] Binomial (float p, int n)
		{
			float q = 1 - p;
			float[] dist = new float[n + 1];
			float P = 1;
			float Q = 1;

			dist[0] = 1;
			for (int i = 0; i < n; i++) {
				dist[i + 1] = dist[i] * (n - i) / (i + 1);
				dist[i] *= P;
				P *= p;
			}
			dist[n] *= P;
			for (int i = n; i >= 0; i--) {
				dist[i] *= Q;
				Q *= q;
			}
			return dist;
		}

		/** Count the number of 1-bits in a value.
		 */
		public static int CountBits (uint x)
		{
			uint count = 0;
			while (x > 0) {
				count += x & 1;
				x >>= 1;
			}
			return (int) count;
		}
	}
}
