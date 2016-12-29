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
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ModName.KerbalStats {
	/**	Wrapper class for accessing KerbalStats via reflection.
	 *
	 *	Removes the need to actually link against KerbalStats.dll.
	 *	Querying KerbalStats from a mod is very easy: include
	 *	KerbalStatsWrapper.cs in your project, change **ModName** in the
	 *	namespace to the name of your mod, and then call KerbalExt.Get() with
	 *	the ProtoCrewMember object representing the kerbal and the query
	 *	string.
	 *
	 *	KerbalStatsWrapper.cs has been writted such that there is no need to
	 *	link against KerbalStats.dll. If the dll is not present, then
	 *	KerbalExt.Get() will return **null** and log the issue.
	 *
	 *	\note KerbalStatsWrapper.cs is licensed using the GNU Lesser General
	 *	Public License (as is the rest of KerbalStats). This means that mods
	 *	are free to use KerbalStatsWrapper.cs without worrying about their own
	 *	license so long as KerbalStatsWrapper.cs itself remains under the GNU
	 *	LGPL.
	 *
	 *	\note It seems some have a poor understanding of the GNU LGPL, so in
	 *	addition to the above, here is an explicit statement: you are free to
	 *	modify and use KerbalStatsWrapper.cs for any purpose on the sole
	 *	condition that KerbalStatsWrapper.cs itself, and any modifications you
	 *	have made to KerbalStatsWrapper.cs, remain free for others to modify
	 *	and use as they see fit. Your work remains yours to license as you see
	 *	fit.
	 */
	public class KerbalExt
	{
		static MethodInfo GetMethod;
		static bool initialized;

		/**	Query a KerbalStats module for extended kerbal information.
		 *
		 *	\param kerbal	The kerbal being queried.
		 *	\param parms	The query string.
		 *	The general format of the query string is
		 *	&laquo;module-name&raquo;:&laquo;module-params&raquo;.
		 *	*module-params* is defined by the module.
		 *
		 *	\return The string-encoded result of the query,
		 *	or **null** if something went wrong. If **null** is returned,
		 *	then something will have been printed to the KSP logs.
		 */
		public static string Get (ProtoCrewMember kerbal, string parms)
		{
			if (!initialized) {
				initialized = true;
				System.Type KStype = AssemblyLoader.loadedAssemblies
					.Select(a => a.assembly.GetTypes())
					.SelectMany(t => t)
					.FirstOrDefault(t => t.FullName == "KerbalStats.KerbalExt");
				if (KStype == null) {
					Debug.LogWarning ("KerbalStats.KerbalExt class not found.");
				} else {
					GetMethod = KStype.GetMethod ("Get", BindingFlags.Public | BindingFlags.Static);
					if (GetMethod == null) {
						Debug.LogWarning ("KerbalExt.Get () not found.");
					}
				}
			}
			if (GetMethod != null) {
				return (string) GetMethod.Invoke (null, new System.Object[]{kerbal, parms});
			}
			return null;
		}
	}
}
