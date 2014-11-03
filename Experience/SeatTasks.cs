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
	public class SeatTasks
	{
		Dictionary <string, string> seats;
		string default_task;

		public string this [string seat]
		{
			get {
				if (seats.ContainsKey (seat)) {
					return seats[seat];
				} else {
					return default_task;
				}
			}
		}

		public SeatTasks (string defTask)
		{
			seats = new Dictionary <string, string> ();
			default_task = defTask;
		}
		public SeatTasks (ConfigNode node)
		{
			if (node.HasValue ("default")) {
				default_task = node.GetValue ("default");
			} else {
				default_task = "Passenger";
			}
			seats = new Dictionary <string, string> ();
			foreach (ConfigNode.Value seat in node.values) {
				if (seat.name == "name" || seat.name == "default") {
					continue;
				}
				seats[seat.name] = seat.value;
			}
		}
	}
}
