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
	public class FemaleFSM : KerbalFSM
	{
		Female female;

		KFSMState state_fertile;
		KFSMState state_pregnant;
		KFSMState state_discovered;
		KFSMState state_resting;
		KFSMState state_dead;

		KFSMEvent event_conceive;
		KFSMEvent event_discover;
		KFSMEvent event_birthe;
		KFSMEvent event_rested;

		bool check_conceive (KFSMState st)
		{
			if (female.location.isWatched ()) {
				return false;
			}
			if (!female.isInterested ()) {
				return false;
			}
			var mate = female.SelectMate (female.location.Males ());
			return mate != null ? female.Mate (mate) : false;
		}

		bool check_discover (KFSMState st)
		{
			return female.DiscoverPregnancy ();
		}

		void report_pregnancy (KFSMState prevState)
		{
			if (prevState == null || prevState == state_discovered) {
				// called from StartFSM (or redundantly?) so already fired
				return;
			}
			//Debug.LogFormat ("report_pregnancy: {0}", female.id);
			ProgenyScenario.current.ReportPregnancy (female);
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

			AddState (state_fertile);
			AddState (state_pregnant);
			AddState (state_discovered);
			AddState (state_resting);
			AddState (state_dead);
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

			AddEvent (event_conceive, new KFSMState [] { state_fertile, });
			AddEvent (event_birthe, new KFSMState [] { state_pregnant, state_discovered});
			AddEvent (event_discover, new KFSMState [] { state_pregnant, });
			AddEvent (event_rested, new KFSMState [] { state_resting, });
		}

		void CreateStateMachine ()
		{
			CreateStates ();
			CreateEvents ();
		}

		void initialize ()
		{
		}

		public FemaleFSM (Female female)
		{
			this.female = female;
			CreateStateMachine ();
		}
	}
}
