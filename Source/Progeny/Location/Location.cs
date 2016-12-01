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

namespace KerbalStats.Progeny {
	public abstract class Location
	{
		Dictionary<string, Zygote> zygotes;	// all kerbals in this location
		Dictionary<string, Male> males;		// males in this location
		Dictionary<string, Female> females;	// females in this location

		public string name { get; protected set; }

		public Location ()
		{
			zygotes = new Dictionary<string, Zygote> ();
			males = new Dictionary<string, Male> ();
			females = new Dictionary<string, Female> ();
		}
		public abstract bool isWatched ();

		public override string ToString ()
		{
			return name;
		}

		public void Add (Zygote zygote)
		{
			zygotes[zygote.id] = zygote;
			if (zygote is Female) {
				females[zygote.id] = zygote as Female;
			} else if (zygote is Male) {
				males[zygote.id] = zygote as Male;
			}
		}

		public void Remove (Zygote zygote)
		{
			zygotes.Remove (zygote.id);
			if (zygote is Female) {
				females.Remove (zygote.id);
			} else if (zygote is Male) {
				males.Remove (zygote.id);
			}
		}

		public List<Male> Males ()
		{
			return males.Values.ToList ();
		}

		public List<Female> Females ()
		{
			return females.Values.ToList ();
		}

		public List<Zygote> Zygotes ()
		{
			return zygotes.Values.ToList ();
		}
	}
}
