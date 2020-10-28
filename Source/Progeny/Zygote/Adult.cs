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

	public class Adult : Zygote, IKerbal
	{
		/** The game kerbal represented by this zygote.
		 *
		 * Valid only if the kerbal has been recruited (or is an
		 * applicant).
		 */
		public ProtoCrewMember kerbal { get; set; }

		/** Universal Time of the kerbal's birth */
		double birthUT;
		/** Universal Time when the kerbal became an adult */
		double adulthoodUT;
		/** Timespan in seconds of the kerbal's "healthy" adult life */
		double aging;

		/** The kerbal's name
		 *
		 * \note not valid (will NRE) if the kerbal has yet to be
		 * recruited. FIXME
		 */
		public string name { get { return kerbal.name; } }

		/** Common initialization
		 */
		void initialize ()
		{
			aging = bioClock.AgingTime (subp);
		}

		/** Initialize from a juvenile kerbal.
		 *
		 * The juvenile has grown up and can now contribute.
		 */
		public Adult (Juvenile juvenile) : base (juvenile)
		{
			birthUT = juvenile.Birth ();
			adulthoodUT = juvenile.Maturation ();
			kerbal = null;		// not yet recruited
			initialize ();
		}

		/** Initialize from a game generated kerbal.
		 */
		public Adult (ProtoCrewMember kerbal) : base (kerbal)
		{
			this.kerbal = kerbal;
			initialize ();
			CalcAdulthood ();
			CalcBirth ();
		}

		/** Maps 0..1 to 0..1, but favoring smaller values.
		 *
		 * This remaps p to favor younger kerbals, but still allow older
		 * ones.
		 */
		double YoungerP (double p)
		{
			// avoid 1.0: bad juju (ln(0))
			// and anything over 1 gets into negative roots
			// however, as p aproaches 1, the result approaches 1
			if (p >= 1) {
				return 1;
			}
			// map 0..1 onto 0..inf via artanh, then feed that into
			// 1 - (x + 1) e^-x
			//
			p = Math.Sqrt ((1 - p) / (1 + p));
			p = 1 - p * (1 - Math.Log (p));
			return p;
		}

		/** Back-calculate how long the kerbal has been an adult.
		 *
		 * Younger kerbals are favored over older ones, but even kerbals
		 * just before their "aging" time might be applicants. The
		 * wording feels a little backwards because the kerbal's age is
		 * being set rather than the kerbal being filtered by age.
		 */
		protected void CalcAdulthood ()
		{
			var UT = Planetarium.GetUniversalTime ();
			double p = genes.random.Range (0, 1f);
			adulthoodUT = UT - aging * YoungerP (p);
		}

		/** Back-calculate the kerbal's birth time.
		 *
		 * This requires the time the kerbal became an adult to be known
		 * (eg, adulthoodUT must be known (call CalcAdulthood() first)).
		 */
		protected void CalcBirth ()
		{
			var p = genes.random.Range (0, 1f);
			birthUT = adulthoodUT - bioClock.MaturationTime (p);
		}

		/** Initialize the adult kerbal from persistence.
		 *
		 * If the adulthood or birth times are unknown, they'll be
		 * reconstructed.
		 */
		public Adult (ConfigNode node) : base (node)
		{
			this.kerbal = null;
			initialize ();
			if (node.HasValue ("adulthoodUT")) {
				double.TryParse (node.GetValue ("adulthoodUT"), out adulthoodUT);
			} else {
				CalcAdulthood ();
			}
			if (node.HasValue ("birthUT")) {
				double.TryParse (node.GetValue ("birthUT"), out birthUT);
			} else {
				CalcBirth ();
			}
		}

		/** Save the adult kerbal to persistence.
		 */
		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("birthUT", birthUT.ToString ("G17"));
			node.AddValue ("adulthoodUT", adulthoodUT.ToString ("G17"));

		}

		/** Fetch the kerbal's time of birth
		 */
		public double Birth ()
		{
			return birthUT;
		}

		/** Fetch the kerbal's time of adulthood
		 */
		public double Adulthood ()
		{
			return adulthoodUT;
		}

		/** Fetch the kerbal's "healthy" adulthood timespan
		 *
		 * The idea is that once the timespan has elapsed, aging effects
		 * will occur (retirement, health degradation, decreased
		 * fertility, whatever).
		 */
		public double Aging ()
		{
			return aging;
		}
	}
}
