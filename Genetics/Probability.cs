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

namespace KerbalStats.Genetics {
	public class DiscreteDistribution
	{
		float[] ranges;
		float total_range;

		public DiscreteDistribution (float[] ranges)
		{
			this.ranges = ranges.ToArray ();
			total_range = 0;
			for (int i = 0; i < ranges.Length; i++) {
				total_range += ranges[i];
			}
		}

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

	public class ContinuousDistribution
	{
		public delegate float DistributionFunction (float x);
		DistributionFunction pdf;
		DistributionFunction cdf;
		float min;
		float max;
		public float precision = 13684;

		public ContinuousDistribution (DistributionFunction pdf, float min, float max, DistributionFunction cdf = null)
		{
			this.pdf = pdf;
			this.cdf = cdf;
			this.min = min;
			this.max = max;
		}

		public float Density (float x)
		{
			return pdf (x);
		}

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
