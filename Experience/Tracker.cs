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

		public static ExperienceTracker instance;

		public ExperienceTracker ()
		{
			instance = this;
			if (partSeatTasks == null) {
				partSeatTasks = new PartSeatTasks ();
			}
			kerbal_experience = new Dictionary<string,Experience> ();
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
				var exp = new ConfigNode ("experience");
				node.AddNode (exp);
				kerbal_experience[kerbal.name].Save (exp);
			}
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

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class KSGenderInit : MonoBehaviour
	{
		void Awake ()
		{
			KerbalExt.AddModule (new ExperienceTracker ());
		}
	}
}
