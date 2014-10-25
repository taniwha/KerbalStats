using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Experience {
	public class ExperienceTracker : IKerbalStats
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

		public void SetSituation (ProtoCrewMember kerbal, double UT, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.SetSituation (UT, situation);
		}

		public void BeginTask (ProtoCrewMember kerbal, double UT, string task, string situation)
		{
			if (!kerbal_experience.ContainsKey (kerbal.name)) {
				AddKerbal (kerbal);
			}
			var exp = kerbal_experience[kerbal.name];
			exp.BeginTask (UT, task, situation);
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
