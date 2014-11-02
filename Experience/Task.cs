using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Experience {
	class Task
	{
		Dictionary <string, Body> bodies;
		string current;

		public void Load (ConfigNode node)
		{
			bodies = new Dictionary <string, Body> ();
			current = node.GetValue ("_current");
			foreach (ConfigNode body_node in node.nodes) {
				bodies[body_node.name] = new Body ();
				bodies[body_node.name].Load (node);
			}
		}

		public void Save (ConfigNode node)
		{
			foreach (var kv in bodies) {
				var body_node = new ConfigNode (kv.Key);
				node.AddNode (body_node);
				kv.Value.Save (body_node);
			}
			if (current != null) {
				node.AddValue ("_current", current);
			}
		}

		public Task ()
		{
			bodies = new Dictionary <string, Body> ();
		}

		void EndSituation (double UT)
		{
			if (current != null) {
				if (bodies.ContainsKey (current)) {
					bodies[current].EndSituation (UT);
				}
			}
		}

		public void SetSituation (double UT, string body, string situation)
		{
			EndSituation (UT);
			current = body;
			if (!bodies.ContainsKey (current)) {
				bodies[current] = new Body ();
			}
			bodies[current].SetSituation (UT, situation);
		}

		public void FinishTask (double UT)
		{
			EndSituation (UT);
		}

		public void BeginTask (double UT, string body, string situation)
		{
			if (body != current) {
				EndSituation (UT);
			}
			current = body;
			if (!bodies.ContainsKey (current)) {
				bodies[current] = new Body ();
			}
			bodies[current].BeginTask (UT, situation);
		}

		public double GetExperience (double UT, string body, string situation)
		{
			double dur = 0;
			if (body == null) {
				foreach (var b in bodies.Values) {
					dur += b.GetExperience (UT, situation);
				}
			} else {
				if (bodies.ContainsKey (body)) {
					dur = bodies[body].GetExperience (UT, situation);
				}
			}
			return dur;
		}
	}
}
