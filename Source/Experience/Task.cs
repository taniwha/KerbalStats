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
				bodies[body_node.name].Load (body_node);
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
