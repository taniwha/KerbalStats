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
