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
	class Experience
	{
		Dictionary<string, Task> tasks;
		HashSet<string> current;

		public string [] Current
		{
			get {
				return current.ToArray ();
			}
		}

		public void Load (ConfigNode node)
		{
			tasks = new Dictionary<string, Task> ();
			current = new HashSet<string> ();
			foreach (ConfigNode task_node in node.nodes) {
				tasks[task_node.name] = new Task ();
				tasks[task_node.name].Load (task_node);
			}
			var task_list = node.GetValue ("_current");
			if (task_list != null) {
				current.UnionWith (task_list.Split (','));
			}
		}

		public void Save (ConfigNode node)
		{
			foreach (var kv in tasks) {
				var task_node = new ConfigNode (kv.Key);
				node.AddNode (task_node);
				kv.Value.Save (task_node);
			}
			if (current.Count > 0) {
				var task_list = String.Join (",", current.ToArray ());
				node.AddValue ("_current", task_list);
			}
		}

		public Experience ()
		{
			tasks = new Dictionary<string, Task> ();
			current = new HashSet<string> ();
		}

		public void SetSituation (double UT, string body, string situation)
		{
			foreach (var task in current) {
				tasks[task].SetSituation (UT, body, situation);
			}
		}

		public void BeginTask (double UT, string task, string body,
							   string situation)
		{
			if (!tasks.ContainsKey (task)) {
				tasks[task] = new Task ();
			}
			current.Add (task);
			tasks[task].BeginTask (UT, body, situation);
		}

		public void FinishTask (double UT, string task)
		{
			if (!tasks.ContainsKey (task)) {
				return;
			}
			current.Remove (task);
			tasks[task].FinishTask (UT);
		}

		public double GetExperience (double UT, string task, string body, string situation)
		{
			double dur = 0;
			if (task == null) {
				foreach (var t in tasks.Values) {
					dur += t.GetExperience (UT, body, situation);
				}
			} else {
				if (tasks.ContainsKey (task)) {
					dur = tasks[task].GetExperience (UT, body, situation);
				}
			}
			return dur;
		}
	}
}
