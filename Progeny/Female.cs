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

namespace KerbalStats.Progeny {
	public class Female : Zygote, IKerbal, IComparable<Female>
	{
		public ProtoCrewMember kerbal
		{
			get;
			private set;
		}

		double lastUpdate;
		double UT;
		double interestTime;
		double interestTC;
		Embryo embryo;

		KFSMState state_fertile;
		KFSMState state_pregnant;
		KFSMState state_resting;
		KFSMState state_dead;

		KFSMEvent event_conceive;
		KFSMEvent event_birthe;
		KFSMEvent event_rested;

		KerbalFSM fsm;

		public string name
		{
			get {
				return kerbal.name;
			}
		}

		bool isWatched ()
		{
		}

		bool check_conceive (KFSMState st)
		{
			//if (!HighLogic.LoadedSceneIsFlight) {
			if (isWatched ()) {
				return false;
			}
			if (!isInterested ()) {
				return false;
			}
			var mate = SelectMate (ProgenyTracker.AvailableMales ());
			return mate != null ? Mate (mate) : false;
		}

		float Interest ()
		{
			if (UT < interestTime) {
				return 0;
			}
			double x = (UT - interestTime) / interestTC;
			return (float) (1 - (x + 1) * Math.Exp (-x));
		}

		bool isInterested ()
		{
			return UnityEngine.Random.Range (0, 1f) < Interest ();
		}

		float Fertility
		{
			get {
				return 0.5f; //FIXME
			}
		}

		Male SelectMate (List<Male> males)
		{
			float [] male_readiness = new float[males.Count + 1];
			male_readiness[0] = 2; //FIXME
			for (int i = 0; i < males.Count; i++) {
				male_readiness[i + 1] = males[i].Interest (UT);
			}
			var dist = new Genome.DiscreteDistribution (male_readiness);
			int ind = dist.Value (UnityEngine.Random.Range (0, 1f)) - 1;
			if (ind < 0) {
				return null;
			}
			return males[ind];
		}

		bool Mate (Male mate)
		{
			interestTime = UT + 600; //FIXME
			mate.Mate (interestTime);
			float conceive_chance = Fertility * mate.Fertility;
			if (UnityEngine.Random.Range (0, 1f) > conceive_chance) {
				return false;
			}
			embryo = new Embryo (this, mate);
			ProgenyScenario.current.AddEmbryo (embryo);
			return true;
		}

		bool check_assigned_conceive (KFSMState st)
		{
			var vessel = ProgenyTracker.KerbalVessel (kerbal);
			if (vessel.loaded) {
				// being watched
				return false;
			}
			if (vessel.isEVA) {
				//FIXME figure out command seats
				// in that suit?
				return false;
			}
			if (!isInterested ()) {
				return false;
			}
			var mate = SelectMate (ProgenyTracker.BoardedMales (vessel));
			return mate != null ? Mate (mate) : false;
		}

		bool check_birthe (KFSMState st)
		{
			return false;
		}

		bool check_rested (KFSMState st)
		{
			return false;
		}

		void CreateStateMachine ()
		{
			fsm = new KerbalFSM ();

			state_fertile = new KFSMState ("Fertile");

			state_pregnant = new KFSMState ("Pregnant");

			state_resting = new KFSMState ("Resting");

			state_dead = new KFSMState ("Dead");

			event_conceive = new KFSMEvent ("Conceive");
			event_conceive.GoToStateOnEvent = state_pregnant;
			event_conceive.OnCheckCondition = check_conceive;
			event_birthe = new KFSMEvent ("Birthe");
			event_birthe.GoToStateOnEvent = state_resting;
			event_birthe.OnCheckCondition = check_birthe;
			event_rested = new KFSMEvent ("Conceive");
			event_rested.GoToStateOnEvent = state_fertile;
			event_rested.OnCheckCondition = check_rested;

			fsm.AddState (state_fertile);
			fsm.AddState (state_pregnant);
			fsm.AddState (state_resting);
			fsm.AddState (state_dead);

			fsm.AddEvent (event_conceive, new KFSMState [] { state_fertile, });
			fsm.AddEvent (event_birthe, new KFSMState [] { state_pregnant, });
			fsm.AddEvent (event_rested, new KFSMState [] { state_resting, });
		}

		void initialize ()
		{
			lastUpdate = Planetarium.GetUniversalTime ();
			CreateStateMachine ();

			interestTime = 0;
			interestTC = 3600;	//FIXME
			embryo = null;
		}

		public Female (Juvenile juvenile) : base (juvenile)
		{
			kerbal = null;	// not yet recruited
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		public Female (ProtoCrewMember kerbal) : base (kerbal)
		{
			this.kerbal = kerbal;
			initialize ();
			fsm.StartFSM ("Fertile");
		}

		public Female (ConfigNode node) : base (node)
		{
			this.kerbal = null;
			initialize ();
			if (node.HasValue ("state")) {
				fsm.StartFSM (node.GetValue ("state"));
			} else {
				fsm.StartFSM ("Fertile");
			}
			if (node.HasValue ("interestTime")) {
				double.TryParse (node.GetValue ("interestTime"), out interestTime);
			}
			if (node.HasValue ("interestTC")) {
				double.TryParse (node.GetValue ("interestTC"), out interestTC);
			}
			if (node.HasValue ("embryo")) {
				var zid = node.GetValue ("embryo");
				embryo = ProgenyScenario.current.GetEmbryo (zid);
			}
		}

		public override void Save (ConfigNode node)
		{
			base.Save (node);
			node.AddValue ("state", fsm.currentStateName);
			node.AddValue ("interestTime", interestTime.ToString ("G17"));
			node.AddValue ("interestTC", interestTC.ToString ("G17"));
			if (embryo != null) {
				node.AddValue ("embryo", embryo.id);
			}
		}

		public void Update ()
		{
			UT = Planetarium.GetUniversalTime ();
			if (UT - lastUpdate < 3600) {
				return;
			}
			fsm.UpdateFSM ();
			lastUpdate = UT;
		}

		public string State
		{
			get {
				return fsm.currentStateName + " " + Interest ();
			}
		}

		public int CompareTo (Female other)
		{
			return name.CompareTo (other.name);
		}
	}
}
