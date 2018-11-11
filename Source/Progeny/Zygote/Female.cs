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
	public class Female : Adult, IComparable<Female>
	{
		Embryo embryo;
		Interest interest;
		Gamete gamete;
		Cycle cycle;

		public bool isInterested ()
		{
			var p = genes.random.Range (0, 1f);
			return p < interest.isInterested (UT);
		}

		public double GameteLife ()
		{
			var p = genes.random.Range (0, 1f);
			return gamete.Life (p);
		}

#region Mating
		public Male SelectMate (List<Male> males)
		{
			/// Use the interest levels of the available males to create
			/// a probability distribution.
			float [] male_readiness = new float[males.Count + 1];
			/// The first slot is reserved for the female deciding she
			/// doesn't want an actual mate after all (no male was
			/// sufficiently interested or interesting).
			male_readiness[0] = cycle.NonmatingFactor (UT);
			for (int i = 0; i < males.Count; i++) {
				/// \todo Factor in inter-kerbal interest (prefered
				/// mate(s) etc).
				male_readiness[i + 1] = males[i].isInterested (UT);
			}
			var dist = new DiscreteDistribution (male_readiness);
			int ind = dist.Value (genes.random.Range (0, 1f)) - 1;
			if (ind < 0) {
				return null;
			}
			return males[ind];
		}

		public bool Mate (Male mate)
		{
			/// Reset the male's interest level based on mating.
			mate.Mate (UT);
			/// Reset the female's interest level based on Mating.
			interest.Mate (UT);
			/// Determine the probability of conception based on timing
			/// within the female's cycle.
			var ot = cycle.OvulationTime;
			float fv = 0, mv = 0;
			if (UT < ot) {
				/// If ovulation has not occurred yet, check against the
				/// male's gamete life-expectancy.
				/// \todo Needs better handling of multiple mates as
				/// currently the first male with sufficiently viable
				/// gametes will "win", while maybe gametes should be
				/// stored until ovulation occurs.
				mv = mate.gamete.Viability (ot - UT);
				fv = gamete.Viability (0);
			} else {
				/// If ovulation has occurred, check against the female's
				/// gamete life-expectancy.
				mv = mate.gamete.Viability (0);
				fv = gamete.Viability (ot - UT);
			}
			float conceive_chance = fv * mv;
			if (genes.random.Range (0, 1f) > conceive_chance) {
				return false;
			}
			/// If conception was successful, create a new embryo using
			/// genes from both parents.
			/// \todo Support multiple embryos.
			embryo = new Embryo (this, mate);
			ProgenyScenario.current.AddEmbryo (embryo);
			return true;
		}
#endregion

#region Initialization (create, save, load)
		void initialize ()
		{
			lastUpdate = Planetarium.GetUniversalTime ();
			CreateStateMachine ();

			interest = new Interest (genes);
			gamete = new Gamete (genes, true, bioClock);
			cycle = new Cycle (genes, bioClock);
			/// \todo Support multiple embryos
			embryo = null;
		}

		/// A juvenile female has matured.
		public Female (Juvenile juvenile) : base (juvenile)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		/// A new kerbal has been added to the system.
		public Female (ProtoCrewMember kerbal) : base (kerbal)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		/** Loading a female adult that is already tracked.
		 *	The adult may be a kerbal in the roster or an "unknown" that is
		 *	waiting to become available to the player.
		 */
		public Female (ConfigNode node) : base (node)
		{
			initialize ();
			interest.Load (node);
			cycle.Load (node);
			if (node.HasValue ("state")) {
				fsm.StartFSM (node.GetValue ("state"));
			} else {
				fsm.StartFSM ("Fertile");
			}
			/// \todo Support multiple embryos
			if (node.HasValue ("embryo")) {
				var zid = node.GetValue ("embryo");
				embryo = ProgenyScenario.current.GetEmbryo (zid);
			}
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			interest.Save (node);
			cycle.Save (node);
			node.AddValue ("state", fsm.currentStateName);
			/// \todo Support multiple embryos
			if (embryo != null) {
				node.AddValue ("embryo", embryo.id);
			}
		}
#endregion

		double lastUpdate;
		double UT;
		public void Update ()
		{
			UT = Planetarium.GetUniversalTime ();
			if (UT - lastUpdate < 3600) {
				return;
			}
			cycle.Update (UT);
			fsm.UpdateFSM ();
			lastUpdate = UT;
		}

#region IComparable
		public int CompareTo (Female other)
		{
			return name.CompareTo (other.name);
		}
#endregion

#region State machine
		KFSMState state_fertile;
		KFSMState state_pregnant;
		KFSMState state_discovered;
		KFSMState state_resting;
		KFSMState state_dead;

		KFSMEvent event_conceive;
		KFSMEvent event_discover;
		KFSMEvent event_birthe;
		KFSMEvent event_rested;

		KerbalFSM fsm;

		/** Handle all the messy business to do with mating.
		 */
		bool check_conceive (KFSMState st)
		{
			/// If the location is watched, no conception related activity
			/// will occur.
			if (location.isWatched ()) {
				return false;
			}
			/// The female needs to be interested.
			if (!isInterested ()) {
				return false;
			}
			/// Find a suitable male if one is available in the female's
			/// location.
			var mate = SelectMate (location.Males ());
			if (mate == null) {
				/// If no mate was found, reset the interest level based
				/// on failing to mate.
				interest.NonMate (UT);
				return false;
			}
			/// Otherwise, actually mate and potentially concieve
			return Mate (mate);
		}

		/** Determine when the female's pregnacy is discovered/reported.
		 *	This is just whether the pregnancy gets reported to the player:
		 *	the female might know but be keeping the pregnacy a secret.
		 */
		bool check_discover (KFSMState st)
		{
			double time = UT - embryo.conceived;
			double period = bioClock.CyclePeriod;
			/// map 0.5 - 1.5 cyles (after concpetion) to 0.02 to 0.98 so
			/// most pregnacies will be discovered around the time of the
			/// first end-of-cycle, but there's always a possibility of
			/// early discovery or even no discovery until birth
			double factor = 3 * (time - period) / period;
			double p = (Math.Tanh (factor) + 1) / 2;

			/// \todo factor in medical facilities: base should be low
			/// probability (assuming secrecy) until mid-pregnacy and then
			/// the above probability with medical facilities (regular
			/// checkups etc)
			if (genes.random.Range (0, 1f) > p) {
				return false;
			}
			return true;
		}

		/** Report the pregnacy if it has not already been reported.
		 */
		void report_pregnancy (KFSMState prevState)
		{
			if (prevState == null || prevState == state_discovered) {
				// called from StartFSM (or redundantly?) so already fired
				return;
			}
			//Debug.LogFormat ("report_pregnancy: {0}", id);
			ProgenyScenario.current.ReportPregnancy (this);
		}

		/** Check whether it is time for the stork to arrive.
		 */
		bool check_birthe (KFSMState st)
		{
			double gestation = UT - embryo.conceived;
			if (gestation > embryo.Birth) {
				ProgenyScenario.current.Mature (embryo);
				cycle.Recuperate (UT);
				embryo = null;
				return true;
			}
			return false;
		}

		/** Check whether the female's body is ready for another round.
		 */
		bool check_rested (KFSMState st)
		{
			if (cycle.Recuperating (UT))
				return false;
			return true;
		}

		void CreateStates ()
		{
			state_fertile = new KFSMState ("Fertile");

			state_pregnant = new KFSMState ("Pregnant");

			state_discovered = new KFSMState ("Discovered");
			state_discovered.OnEnter = report_pregnancy;

			state_resting = new KFSMState ("Resting");

			state_dead = new KFSMState ("Dead");

			fsm.AddState (state_fertile);
			fsm.AddState (state_pregnant);
			fsm.AddState (state_discovered);
			fsm.AddState (state_resting);
			fsm.AddState (state_dead);
		}

		void CreateEvents ()
		{
			event_conceive = new KFSMEvent ("Conceive");
			event_conceive.GoToStateOnEvent = state_pregnant;
			event_conceive.OnCheckCondition = check_conceive;

			event_discover = new KFSMEvent ("Discover");
			event_discover.GoToStateOnEvent = state_discovered;
			event_discover.OnCheckCondition = check_discover;

			event_birthe = new KFSMEvent ("Birthe");
			event_birthe.GoToStateOnEvent = state_resting;
			event_birthe.OnCheckCondition = check_birthe;

			event_rested = new KFSMEvent ("Rested");
			event_rested.GoToStateOnEvent = state_fertile;
			event_rested.OnCheckCondition = check_rested;

			fsm.AddEvent (event_conceive, new KFSMState [] { state_fertile, });
			fsm.AddEvent (event_birthe, new KFSMState [] { state_pregnant, state_discovered});
			fsm.AddEvent (event_discover, new KFSMState [] { state_pregnant, });
			fsm.AddEvent (event_rested, new KFSMState [] { state_resting, });
		}

		void CreateStateMachine ()
		{
			fsm = new KerbalFSM ();
			CreateStates ();
			CreateEvents ();
		}

		public string State
		{
			get {
				UT = Planetarium.GetUniversalTime ();
				return fsm.currentStateName + " " + interest.isInterested (UT);
			}
		}
#endregion
	}
}
