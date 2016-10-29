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

namespace KerbalStats.Experience {
	public class ExperienceTracker : IKerbalExt
	{
		Dictionary<string,Experience> kerbal_experience;
		public static PartSeatTasks partSeatTasks;

		ExperienceTrackerEvents event_handler;
		public static ExperienceTracker instance;

		public ExperienceTracker (Scenario.KerbalStats ks)
		{
			instance = this;
			event_handler = new ExperienceTrackerEvents (this);
			if (partSeatTasks == null) {
				partSeatTasks = new PartSeatTasks ();
			}
			Clear ();
		}

		public void AddKerbal (ProtoCrewMember kerbal)
		{
			kerbal_experience[kerbal.name] = new Experience ();
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
			kerbal_experience.Remove (kerbal.name);
		}

		public string name
		{
			get {
				return "experience";
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			kerbal_experience[kerbal.name] = new Experience ();
			if (node.HasNode (name)) {
				var exp = node.GetNode (name);
				kerbal_experience[kerbal.name].Load (exp);
			} else {
				AddKerbal (kerbal);
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (kerbal_experience.ContainsKey (kerbal.name)) {
				var exp = new ConfigNode (name);
				node.AddNode (exp);
				kerbal_experience[kerbal.name].Save (exp);
			}
		}

		public void Clear ()
		{
			kerbal_experience = new Dictionary<string,Experience> ();
		}

		public void Shutdown ()
		{
			event_handler.Shutdown ();
			event_handler = null;
			instance = null;
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				Debug.LogError ("[KS] ExperienceTracker.Get: no such kerbal: " + kerbal.name);
				return null;
			}
			string task = null;
			string body = null;
			string situation = null;
			if (parms != "") {
				string [] param_list = parms.Split (',');
				for (int i = 0; i < param_list.Count (); i++) {
					string [] args = param_list[i].Split ('=');
					if (args.Count () == 2) {
						if (args[0] == "task") {
							task = args[1];
						} else if (args[0] == "body") {
							body = args[1];
						} else if (args[0] == "situation") {
							situation = args[1];
						} else {
							Debug.LogError ("[KS] ExperienceTracker.Get: invalid keyword" + args[0]);
						}
					} else {
						Debug.LogError ("[KS] ExperienceTracker.Get: invalid param" + param_list[i]);
					}
				}
			}
			double UT = Planetarium.GetUniversalTime ();
			var exp = kerbal_experience[kerbal.name].GetExperience (UT, task, body, situation);
			return exp.ToString ("G17");
		}

		public void SetSituation (ProtoCrewMember kerbal, double UT,
								  string body, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.SetSituation (UT, body, situation);
		}

		public void BeginTask (ProtoCrewMember kerbal, double UT, string task,
							   string body, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.BeginTask (UT, task, body, situation);
		}

		public void FinishTask (ProtoCrewMember kerbal, double UT, string task)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.FinishTask (UT, task);
		}

		public void FinishAllTasks (ProtoCrewMember kerbal, double UT)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			foreach (var task in exp.Current) {
				exp.FinishTask (UT, task);
			}
		}
	}
}
