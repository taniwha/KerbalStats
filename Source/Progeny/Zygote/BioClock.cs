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
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny.Zygotes {
	using Genome;
	using Traits;

	/** The kerbal's central "clock".
	 *
	 * The idea is the relative speed of the bio-clock sets the speeds
	 * of all the processes. Individual tuning comes from shape (K)
	 * and range (P) adjusters.
	 *
	 * All times are in seconds.
	 */
	public class BioClock
	{
		/** Cached reference to the bioClockTC trait
		 */
		BioClockTC bc_trait;
		/** Gene-pair that controls Bio-clock time constant.
		 *
		 * Depending on bioClockInverse, either slows down or speeds up
		 * the bio-clock.
		 */
		GenePair bioClockTC;
		/** Gene-pair that controls the bio-clock speed direction.
		 */
		GenePair bioClockInverse;
		/** Cached maturation curve shape
		 *
		 * \see Traits.TimeK
		 */
		double maturationK;
		/** Cached maturation P-range
		 *
		 * \see Traits.TimeP
		 */
		PRange maturationP;
		/** Cached aging curve shape
		 *
		 * \see Traits.TimeK
		 */
		double agingK;
		/** Cached aging P-range
		 *
		 * \see Traits.TimeP
		 */
		PRange agingP;

		/** Initialize from a set of gene pairs.
		 *
		 * Caches all the required values.
		 */
		public BioClock (GenePair[] genes)
		{
			for (int i = 0; i < genes.Length; i++) {
				var g = genes[i];
				switch (g.trait.name) {
					case "BioClockTC":
						bioClockTC = g;
						break;
					case "BioClockInverse":
						bioClockInverse = g;
						break;
					case "MaturationTimeK":
						maturationK = (g.trait as TimeK).K (g);
						break;
					case "MaturationTimeP":
						maturationP = (g.trait as TimeP).P (g);
						break;
					case "AgingTimeK":
						agingK = (g.trait as TimeK).K (g);
						break;
					case "AgingTimeP":
						agingP = (g.trait as TimeP).P (g);
						break;
				}
			}
			bc_trait = bioClockTC.trait as BioClockTC;
		}

		/** Determine the ovulation time lambda
		 *
		 * Average time within a cycle at which ovulation occurs.
		 */
		public double OvulationTime
		{
			get {
				return bc_trait.OvulationTime (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the recuperation time lambda
		 *
		 * Average length of time it takes for the female to become
		 * fertile again after giving birth.
		 */
		public double RecuperationTime
		{
			get {
				return bc_trait.RecuperationTime (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the cycle period lambda
		 *
		 * Average length of the female's cycle.
		 */
		public double CyclePeriod
		{
			get {
				return bc_trait.CyclePeriod (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the egg lifespan lambda
		 *
		 * Average length of time the egg remains viable after ovulation.
		 */
		public double EggLife
		{
			get {
				return bc_trait.EggLife (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the sperm lifespan lambda
		 */
		public double SpermLife
		{
			get {
				return bc_trait.SpermLife (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the gestation period lambda
		 */
		public double GestationPeriod
		{
			get {
				return bc_trait.GestationPeriod (bioClockTC, bioClockInverse);
			}
		}

		/** Determine the aging time
		 *
		 * The aging time is the length of "healthy" adulthood.
		 *
		 * \param p     Selection parameter, 0..1. Combined with the
		 *              range adjuster determines the overall selection
		 *              parameter.
		 * \return      The length of time from maturation to aging.
		 */
		public double AgingTime (double p)
		{
			double lambda = bc_trait.AgingTime (bioClockTC, bioClockInverse);
			p = agingP.P (p);
			return MathUtil.WeibullQF (lambda, agingK, p);
		}

		/** Determine the maturation time
		 *
		 * This is how long from birth until the kerbal becomes an
		 * adult.
		 *
		 * \param p     Selection parameter, 0..1. Combined with the
		 *              range adjuster determines the overall selection
		 *              parameter.
		 * \return      The length of time from birth to maturation.
		 */
		public double MaturationTime (double p)
		{
			double lambda = bc_trait.MaturationTime (bioClockTC, bioClockInverse);
			p = maturationP.P (p);
			return MathUtil.WeibullQF (lambda, maturationK, p);
		}
	}
}
