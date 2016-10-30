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
		public static PartSeatTasks partSeatTasks;

		ExperienceTrackerEvents event_handler;
		public static ExperienceTracker instance;

		public ExperienceTracker (KerbalStats ks)
		{
			instance = this;
			event_handler = new ExperienceTrackerEvents (this);
			if (partSeatTasks == null) {
				partSeatTasks = new PartSeatTasks ();
			}
			Clear ();
		}

		public void AddKerbal (KerbalExt kerbal)
		{
			kerbal[name] = new Experience ();
		}

		public void RemoveKerbal (KerbalExt kerbal)
		{
		}

		public string name
		{
			get {
				return "experience";
			}
		}

		public void Load (KerbalExt kerbal, ConfigNode node)
		{
			var experience = new Experience ();
			kerbal[name] = experience;
			if (node.HasNode (name)) {
				var exp = node.GetNode (name);
				experience.Load (exp);
			} else {
				AddKerbal (kerbal);
			}
		}

		public void Save (KerbalExt kerbal, ConfigNode node)
		{
			Experience experience = kerbal[name] as Experience;
			var exp = new ConfigNode (name);
			node.AddNode (exp);
			experience.Save (exp);
		}

		public void Clear ()
		{
		}

		public void Shutdown ()
		{
			event_handler.Shutdown ();
			event_handler = null;
			instance = null;
		}

		public string Get (KerbalExt kerbal, string parms)
		{
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
			var exp = (kerbal[name] as Experience).GetExperience (UT, task, body, situation);
			return exp.ToString ("G17");
		}

		public void SetSituation (ProtoCrewMember pcm, double UT,
								  string body, string situation)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			var exp = kerbal[name] as Experience;
			exp.SetSituation (UT, body, situation);
		}

		public void BeginTask (ProtoCrewMember pcm, double UT, string task,
							   string body, string situation)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			var exp = kerbal[name] as Experience;
			exp.BeginTask (UT, task, body, situation);
		}

		public void FinishTask (ProtoCrewMember pcm, double UT, string task)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			var exp = kerbal[name] as Experience;
			exp.FinishTask (UT, task);
		}

		public void FinishAllTasks (ProtoCrewMember pcm, double UT)
		{
			KerbalExt kerbal = KerbalStats.current[pcm];
			var exp = kerbal[name] as Experience;
			foreach (var task in exp.Current) {
				exp.FinishTask (UT, task);
			}
		}
	}
}
