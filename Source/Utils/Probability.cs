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

namespace KerbalStats {
	/** Represent a discrete probability distribution
	 */
	public class DiscreteDistribution
	{
		float[] ranges;
		float total_range;

		/** Initialize the distribution from the array of ranges
		 *
		 * \param ranges    array of sized bins. Does not need to add up to
		 *                  1 as the total range is kept track of.
		 */
		public DiscreteDistribution (float[] ranges)
		{
			this.ranges = ranges.ToArray ();
			total_range = 0;
			for (int i = 0; i < ranges.Length; i++) {
				total_range += ranges[i];
			}
		}

		/** Choose a random item from the discrete distribution.
		 *
		 * \param p     The random selection value. Should be between 0 and 1
		 * \return      Index of the range that exceeds p. If p >= 1, then the
		 *              final index is returned.
		 */
		public int Value (float p)
		{
			// p is assumed to be 0..1, but the total range can be anything,
			// so map p to the distribution.
			p *= total_range;
			for (int i = 0; i < ranges.Length; i++) {
				if (p < ranges[i]) {
					return i;
				}
				p -= ranges[i];
			}
			return ranges.Length - 1;
		}
	}

	/** Represent a continuous probability distribution
	 */
	public class ContinuousDistribution
	{
		/** Delegate for the distribution functions (probability density and
		 * cumulative distribution)
		 */
		public delegate float DistributionFunction (float x);
		/** Probability density function
		 */
		DistributionFunction pdf;
		/** Cumulative distribution function (if available).
		 *
		 * If not available (because the integral is too difficult), the pdf
		 * will be numerically integrated to find the desired result.
		 */
		DistributionFunction cdf;
		/** Minimum value of range to search when numerically integrating.
		 */
		float min;
		/** Maximum value of range to search when numerically integrating.
		 */
		float max;
		/** Number of slots in the range to search when numerically integrating.
		 *
		 * Not used if cdf is valid.
		 */
		public float precision = 13684;

		/** Create the continuous distribution.
		 *
		 * \param pdf   The probability desnity function of the distribution
		 * \param min   The minimum value of the distribution.
		 * \param max   The maximum value of the distribution.
		 * \param cdf   The cumulative distribution function (integral of pdf).
		 *              If not given, the pdf is numerically integrated when
		 *              necessary. However, if available, a cdf should be used
		 *              as it will generally be much faster and more precise.
		 */
		public ContinuousDistribution (DistributionFunction pdf, float min, float max, DistributionFunction cdf = null)
		{
			this.pdf = pdf;
			this.cdf = cdf;
			this.min = min;
			this.max = max;
		}

		/** Return the probability density at a point on the distribution.
		 */
		public float Density (float x)
		{
			return pdf (x);
		}

		/** Return the cumulative probabitlity at a point on the distribution.
		 *
		 * Represents the probability of a value smaller than x.
		 */
		public float Cumulation (float x)
		{
			if (cdf != null) {
				return cdf (x);
			} else {
				float c = 0;
				float step = (max - min) / precision;
				float y = min;
				while (y < x) {
					c += (pdf (y) + pdf (y + step)) * step / 2;
					y += step;
				}
				return c;
			}
		}

		/** Choose a random value from the continuous distribution.
		 *
		 * \param p     The random selection value. Should be between 0 and 1
		 * \return      The value for which the cdf is close to the selection
		 *              value (ie, within (max - min) / precision. The returned
		 *              value is somewhere between min and max.
		 *              If the cdf is not available, the pdf is numerically
		 *              integrated to find the value.
		 */
		public float Value (float p)
		{
			if (cdf != null) {
				float a = min;
				float b = max;
				float e = (max - min) / precision;
				while (true) {
					float x = (a + b) / 2;
					float c = cdf (x);
					if (c - p > e) {
						b = x;
					} else if (p - c > e) {
						a = x;
					} else {
						return x;
					}
				}
			} else {
				float c = 0;
				float x;
				float step = (max - min) / precision;
				for (x = min; x < max; x += step) {
					c += (pdf (x) + pdf (x + step)) * step / 2;
					if (c >= p) {
						break;
					}
				}
				if (x > max) {
					x = max;
				}
				return x;
			}
		}
	}
}
