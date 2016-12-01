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
	public class PartSeatTasks
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
