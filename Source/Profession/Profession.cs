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

namespace KerbalStats.Profession {
	public class ProfessionTracker : IKerbalExt
	{
		public ProfessionTracker (KerbalStats ks)
		{
		}

		public void AddKerbal (ProtoCrewMember kerbal)
		{
		}

		public void RemoveKerbal (ProtoCrewMember kerbal)
		{
		}

		public string name
		{
			get {
				return "profession";
			}
		}

		public void Load (ProtoCrewMember kerbal, ConfigNode node)
		{
			if (node.HasNode (name)) {
				var trait = node.GetNode (name);
				string traitName = trait.GetValue ("current");
				KerbalRoster.SetExperienceTrait (kerbal, traitName);
			}
		}

		public void Save (ProtoCrewMember kerbal, ConfigNode node)
		{
			var trait = new ConfigNode (name);
			node.AddNode (trait);
			trait.AddValue ("current", kerbal.experienceTrait.TypeName);
		}

		public void Clear ()
		{
			// nothing to clear
		}

		public void Shutdown ()
		{
		}

		public string Get (ProtoCrewMember kerbal, string parms)
		{
			Debug.LogError ("[KS] ProfessionTracker.Get: stock feature enhancement");
			return null;
		}
	}
}
