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
	public class Female
	{
		ProtoCrewMember kerbal;
		double lastUpdate;
		double UT;
		bool update_pending;
		ProtoCrewMember.RosterStatus oldStatus;

		KFSMState state_available_fertile;
		KFSMState state_available_pregnant;
		KFSMState state_available_resting;

		KFSMState state_assigned_fertile;
		KFSMState state_assigned_pregnant;
		KFSMState state_assigned_resting;

		KFSMState state_missing_fertile;
		KFSMState state_missing_pregnant;
		KFSMState state_missing_resting;

		KFSMState state_dead;

		KFSMEvent event_available_conceive;
		KFSMEvent event_available_birthe;
		KFSMEvent event_available_rested;

		KFSMEvent event_assigned_conceive;
		KFSMEvent event_assigned_birthe;
		KFSMEvent event_assigned_rested;

		KFSMEvent event_missing_conceive;
		KFSMEvent event_missing_birthe;
		KFSMEvent event_missing_rested;

		KFSMEvent event_assigned;
		KFSMEvent event_lost;
		KFSMEvent event_recover;
		KFSMEvent event_die;

		KerbalFSM fsm;

		public string name
		{
			get {
				return kerbal.name;
			}
		}

		void enter_fertile_states (KFSMState st)
		{
			event_assigned.GoToStateOnEvent = state_assigned_fertile;
			event_recover.GoToStateOnEvent = state_available_fertile;
			event_lost.GoToStateOnEvent = state_missing_fertile;
		}

		void enter_pregnant_states (KFSMState st)
		{
			event_assigned.GoToStateOnEvent = state_assigned_pregnant;
			event_recover.GoToStateOnEvent = state_available_pregnant;
			event_lost.GoToStateOnEvent = state_missing_pregnant;
		}

		void enter_resting_states (KFSMState st)
		{
			event_assigned.GoToStateOnEvent = state_assigned_resting;
			event_recover.GoToStateOnEvent = state_available_resting;
			event_lost.GoToStateOnEvent = state_missing_resting;
		}

		bool check_available_conceive (KFSMState st)
		{
			return false;
		}

		bool check_assigned_conceive (KFSMState st)
		{
			if (kerbal.KerbalRef == null || kerbal.KerbalRef.InPart == null
				|| kerbal.KerbalRef.InPart.vessel == null) {
				return false;
			}
			var vessel = kerbal.KerbalRef.InPart.vessel;
			if (vessel.loaded) {
				// being watched
				return false;
			}
			if (vessel.isEVA) {
				//FIXME figure out command seats
				// in that suit?
				return false;
			}
			return false;
		}

		bool check_missing_conceive (KFSMState st)
		{
			return false;
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

			state_available_fertile = new KFSMState ("Available:Fertile");
			state_available_fertile.OnEnter = enter_fertile_states;

			state_available_pregnant = new KFSMState ("Available:Pregnant");
			state_available_pregnant.OnEnter = enter_pregnant_states;

			state_available_resting = new KFSMState ("Available:Resting");
			state_available_resting.OnEnter = enter_resting_states;


			state_assigned_fertile = new KFSMState ("Assigned:Fertile");
			state_assigned_fertile.OnEnter = enter_fertile_states;

			state_assigned_pregnant = new KFSMState ("Assigned:Pregnant");
			state_assigned_pregnant.OnEnter = enter_pregnant_states;

			state_assigned_resting = new KFSMState ("Assigned:Resting");
			state_assigned_resting.OnEnter = enter_resting_states;


			state_missing_fertile = new KFSMState ("Missing:Fertile");
			state_missing_fertile.OnEnter = enter_fertile_states;

			state_missing_pregnant = new KFSMState ("Missing:Pregnant");
			state_missing_pregnant.OnEnter = enter_pregnant_states;

			state_missing_resting = new KFSMState ("Missing:Resting");
			state_missing_resting.OnEnter = enter_resting_states;


			state_dead = new KFSMState ("Dead");

			event_available_conceive = new KFSMEvent ("Available:Conceive");
			event_available_conceive.GoToStateOnEvent = state_available_pregnant;
			event_available_conceive.OnCheckCondition = check_available_conceive;
			event_available_birthe = new KFSMEvent ("Available:Birthe");
			event_available_birthe.GoToStateOnEvent = state_available_resting;
			event_available_birthe.OnCheckCondition = check_birthe;
			event_available_rested = new KFSMEvent ("Available:Conceive");
			event_available_rested.GoToStateOnEvent = state_available_fertile;
			event_available_rested.OnCheckCondition = check_rested;

			event_assigned_conceive = new KFSMEvent ("Assigned:Conceive");
			event_assigned_conceive.GoToStateOnEvent = state_assigned_pregnant;
			event_assigned_conceive.OnCheckCondition = check_assigned_conceive;
			event_assigned_birthe = new KFSMEvent ("Assigned:Birthe");
			event_assigned_birthe.GoToStateOnEvent = state_assigned_resting;
			event_assigned_birthe.OnCheckCondition = check_birthe;
			event_assigned_rested = new KFSMEvent ("Assigned:Conceive");
			event_assigned_rested.GoToStateOnEvent = state_assigned_fertile;
			event_assigned_rested.OnCheckCondition = check_rested;

			event_missing_conceive = new KFSMEvent ("Missing:Conceive");
			event_missing_conceive.GoToStateOnEvent = state_missing_pregnant;
			event_missing_conceive.OnCheckCondition = check_missing_conceive;
			event_missing_birthe = new KFSMEvent ("Missing:Birthe");
			event_missing_birthe.GoToStateOnEvent = state_missing_resting;
			event_missing_birthe.OnCheckCondition = check_birthe;
			event_missing_rested = new KFSMEvent ("Missing:Conceive");
			event_missing_rested.GoToStateOnEvent = state_missing_fertile;
			event_missing_rested.OnCheckCondition = check_rested;

			event_assigned = new KFSMEvent ("Assigned");
			event_recover = new KFSMEvent ("Recover");
			event_lost = new KFSMEvent ("Lost");
			event_die = new KFSMEvent ("Die");

			fsm.AddState (state_available_fertile);
			fsm.AddState (state_available_pregnant);
			fsm.AddState (state_available_resting);
			fsm.AddState (state_assigned_fertile);
			fsm.AddState (state_assigned_pregnant);
			fsm.AddState (state_assigned_resting);
			fsm.AddState (state_missing_fertile);
			fsm.AddState (state_missing_pregnant);
			fsm.AddState (state_missing_resting);
			fsm.AddState (state_dead);

			fsm.AddEvent (event_available_conceive, new KFSMState [] { state_available_fertile, });
			fsm.AddEvent (event_available_birthe, new KFSMState [] { state_available_pregnant, });
			fsm.AddEvent (event_available_rested, new KFSMState [] { state_available_resting, });

			fsm.AddEvent (event_assigned_conceive, new KFSMState [] { state_assigned_fertile, });
			fsm.AddEvent (event_assigned_birthe, new KFSMState [] { state_assigned_pregnant, });
			fsm.AddEvent (event_assigned_rested, new KFSMState [] { state_assigned_resting, });

			fsm.AddEvent (event_missing_conceive, new KFSMState [] { state_missing_fertile, });
			fsm.AddEvent (event_missing_birthe, new KFSMState [] { state_missing_pregnant, });
			fsm.AddEvent (event_missing_rested, new KFSMState [] { state_missing_resting, });

			fsm.AddEvent (event_assigned, new KFSMState [] {
				state_available_fertile,
				state_available_pregnant,
				state_available_resting,
			});

			fsm.AddEvent (event_recover, new KFSMState [] {
				state_assigned_fertile,
				state_assigned_pregnant,
				state_assigned_resting,
				state_missing_fertile,
				state_missing_pregnant,
				state_missing_resting,
			});

			fsm.AddEvent (event_lost, new KFSMState [] {
				state_assigned_fertile,
				state_assigned_pregnant,
				state_assigned_resting,
			});

			fsm.AddEvent (event_die, new KFSMState [] {
				state_available_fertile,
				state_available_pregnant,
				state_available_resting,
				state_assigned_fertile,
				state_assigned_pregnant,
				state_assigned_resting,
				state_missing_fertile,
				state_missing_pregnant,
				state_missing_resting,
			});
		}

		public Female (ProtoCrewMember kerbal)
		{
			this.kerbal = kerbal;
			lastUpdate = Planetarium.GetUniversalTime ();
			GameEvents.onKerbalStatusChange.Add (onKerbalStatusChange);
		}

		public Female (ProtoCrewMember kerbal, ConfigNode progeny)
		{
			this.kerbal = kerbal;
			lastUpdate = Planetarium.GetUniversalTime ();
			GameEvents.onKerbalStatusChange.Add (onKerbalStatusChange);
		}

		internal void Save (ConfigNode progeny)
		{
		}

		~Female ()
		{
			GameEvents.onKerbalStatusChange.Remove (onKerbalStatusChange);
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

		void onKerbalStatusChange (ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
		{
			if (kerbal != this.kerbal || newStatus == oldStatus) {
				return;
			}
			KSProgenyRunner.instance.StartCoroutine (DelayStatusUpdate ());
		}

		internal IEnumerator<YieldInstruction> DelayStatusUpdate ()
		{
			if (update_pending) {
				yield break;
			}
			oldStatus = kerbal.rosterStatus;
			update_pending = true;
			yield return null;
			yield return null;
			if (kerbal.rosterStatus == oldStatus) {
				yield break;
			}
			switch (kerbal.rosterStatus) {
				case ProtoCrewMember.RosterStatus.Available:
					fsm.RunEvent (event_recover);
					break;
				case ProtoCrewMember.RosterStatus.Assigned:
					fsm.RunEvent (event_assigned);
					break;
				case ProtoCrewMember.RosterStatus.Dead:
					fsm.RunEvent (event_die);
					break;
				case ProtoCrewMember.RosterStatus.Missing:
					fsm.RunEvent (event_lost);
					break;
			}
		}
	}
}
