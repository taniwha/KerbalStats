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
			var p = UnityEngine.Random.Range (0, 1f);
			return p < interest.isInterested (UT);
		}

		public double GameteLife ()
		{
			var p = UnityEngine.Random.Range (0, 1f);
			return gamete.Life (p);
		}

		public Male SelectMate (List<Male> males)
		{
			float [] male_readiness = new float[males.Count + 1];
			male_readiness[0] = cycle.NonmatingFactor (UT);
			for (int i = 0; i < males.Count; i++) {
				male_readiness[i + 1] = males[i].isInterested (UT);
			}
			var dist = new Genome.DiscreteDistribution (male_readiness);
			int ind = dist.Value (UnityEngine.Random.Range (0, 1f)) - 1;
			if (ind < 0) {
				return null;
			}
			return males[ind];
		}

		public bool Mate (Male mate)
		{
			mate.Mate (UT);
			interest.Mate (UT);
			var ot = cycle.OvulationTime;
			float fv = 0, mv = 0;
			if (UT < ot) {
				mv = mate.gamete.Viability (ot - UT);
				fv = gamete.Viability (0);
			} else {
				mv = mate.gamete.Viability (0);
				fv = gamete.Viability (ot - UT);
			}
			float conceive_chance = fv * mv;
			if (UnityEngine.Random.Range (0, 1f) > conceive_chance) {
				return false;
			}
			embryo = new Embryo (this, mate);
			ProgenyScenario.current.AddEmbryo (embryo);
			return true;
		}

		void initialize ()
		{
			lastUpdate = Planetarium.GetUniversalTime ();
			CreateStateMachine ();

			interest = new Interest (genes);
			gamete = new Gamete (genes, true, bioClock);
			cycle = new Cycle (genes, bioClock);
			embryo = null;
		}

		public Female (Juvenile juvenile) : base (juvenile)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		public Female (ProtoCrewMember kerbal) : base (kerbal)
		{
			initialize ();
			fsm.StartFSM ("Fertile");
		}

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
			if (embryo != null) {
				node.AddValue ("embryo", embryo.id);
			}
		}

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

		public int CompareTo (Female other)
		{
			return name.CompareTo (other.name);
		}


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

		bool check_conceive (KFSMState st)
		{
			if (location.isWatched ()) {
				return false;
			}
			if (!isInterested ()) {
				return false;
			}
			var mate = SelectMate (location.Males ());
			return mate != null ? Mate (mate) : false;
		}

		bool check_discover (KFSMState st)
		{
			double time = UT - embryo.conceived;
			double period = bioClock.CyclePeriod;
			// map 0.5 - 1.5 cyles (after concpetion) to 0.02 to 0.98 so most
			// pregnacies will be discovered around the time of the first
			// end-of-cycle, but there's always a possibility of early
			// discovery or even no discovery until birth
			double factor = 3 * (time - period) / period;
			double p = (Math.Tanh (factor) + 1) / 2;

			// FIXME factor in medical facilities: base should be low
			// probability (assuming secrecy) until mid-pregnacy and then
			// the above probability with medical facilities (regular checkups
			// etc)
			if (UnityEngine.Random.Range (0, 1f) > p) {
				return false;
			}
			return true;
		}

		void report_pregnancy (KFSMState prevState)
		{
			if (prevState == null || prevState == state_discovered) {
				// called from StartFSM (or redundantly?) so already fired
				return;
			}
			//Debug.LogFormat ("report_pregnancy: {0}", id);
			ProgenyScenario.current.ReportPregnancy (this);
		}

		bool check_birthe (KFSMState st)
		{
			return false;
		}

		bool check_rested (KFSMState st)
		{
			return false;
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
	}
}
