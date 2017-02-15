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

namespace KerbalStats.Progeny.Locations {
	public class VesselPart : Location
	{
		Vessel _vessel;
		public Vessel vessel
		{
			get {
				return _vessel;
			}
			set {
				_vessel = value;
				if (value != null) {
					name = "VesselPart" + value.name;
				}
			}
		}

		public VesselPart (Vessel v)
		{
			vessel = v;
		}

		public override bool isWatched ()
		{
			return vessel.loaded;
		}

		public override string ToString ()
		{
			return "VesselPart," + vessel.id.ToString("N");
		}
	}
}
