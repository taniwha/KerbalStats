using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Experience {
	class PartSeatTasks
	{
		Dictionary <string, SeatTasks> partSeatTasks;
		SeatTasks default_seatTask;

		public SeatTasks this [string part]
		{
			get {
				if (partSeatTasks.ContainsKey (part)) {
					return partSeatTasks[part];
				} else {
					return default_seatTask;
				}
			}
		}

		public PartSeatTasks ()
		{
			var dbase = GameDatabase.Instance;
			default_seatTask = new SeatTasks ("Passenger");
			partSeatTasks = new Dictionary <string, SeatTasks> ();
			foreach (var seatMap in dbase.GetConfigNodes ("KSExpSeatMap")) {
				foreach (var partSeatMap in seatMap.GetNodes ("SeatTasks")) {
					string name = partSeatMap.GetValue ("name");
					if (name == null) {
						continue;
					}
					partSeatTasks[name] = new SeatTasks (partSeatMap);
				}
			}
		}
	}
}
