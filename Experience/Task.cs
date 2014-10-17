using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Experience {
	class Task
	{
		Dictionary <string, double> situations;
		double currentUT;
		string current;

		public void Load (ConfigNode node)
		{
			situations = new Dictionary <string, double> ();
			current = node.GetValue ("_current");
			if (current != null) {
				var sut = node.GetValue ("_currentUT");
				if (!double.TryParse (sut, out currentUT)) {
					currentUT = 0;
					current = null;
				}
			}
			foreach (ConfigNode.Value value in node.values) {
				if (value.name[0] != '_') {
					double dur;
					if (double.TryParse (value.value, out dur)) {
						situations[value.name] = dur;
					}
				}
			}
		}

		public void Save (ConfigNode node)
		{
			if (current != null) {
				node.AddValue ("_current", current);
				node.AddValue ("_currentUT", currentUT);
			}
			foreach (var kv in situations) {
				node.AddValue (kv.Key, kv.Value.ToString ("G17"));
			}
		}

		public Task ()
		{
			situations = new Dictionary <string, double> ();
		}

		void EndSituation (double UT)
		{
			if (current != null) {
				if (!situations.ContainsKey (current)) {
					situations[current] = 0;
				}
				situations[current] += UT - currentUT;
			}
			current = null;
		}

		public void SetSituation (double UT, string situation)
		{
			EndSituation (UT);
			current = situation;
			currentUT = UT;
		}

		public void FinishTask (double UT)
		{
			EndSituation (UT);
		}

		public void BeginTask (double UT, string situation)
		{
			SetSituation (UT, situation);
		}

		public double Experience (double UT)
		{
			double exp = 0;
			foreach (var dur in situations.Values) {
				exp += dur;
			}
			if (current != null) {
				exp += UT - currentUT;
			}
			return exp;
		}

		public double Experience (double UT, string sit)
		{
			double exp = 0;
			if (situations.ContainsKey (sit)) {
				exp += situations[sit];
			}
			if (current == sit) {
				exp += UT - currentUT;
			}
			return exp;
		}
	}
}
