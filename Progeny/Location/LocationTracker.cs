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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalStats.Progeny {
	public class LocationTracker
	{
		Dictionary<Guid, VesselPart> vessel_parts;

		AstronautComplex astronaut_complex;
		EVA eva;
		Wilds wilds;
		Womb womb;
		Tomb tomb;

		public Location location (string loc, object parm)
		{
			switch (loc) {
				case "Vessel":
					Vessel v = parm as Vessel;
					if (!vessel_parts.ContainsKey (v.id)) {
						vessel_parts[v.id] = new VesselPart (v);
					}
					return vessel_parts[v.id];
				case "EVA":
					return eva;
				case "Wilds":
					return wilds;
				case "Womb":
					return womb;
				case "Tomb":
					return tomb;
				case "AstronautComplex":
					return astronaut_complex;
			}
			return null;
		}

		public LocationTracker ()
		{
			vessel_parts = new Dictionary<Guid, VesselPart> ();
			astronaut_complex = new AstronautComplex ();
			eva = new EVA ();
			wilds = new Wilds ();
			womb = new Womb ();
			tomb = new Tomb ();
		}
	}
}
