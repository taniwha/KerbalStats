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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalStats {
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
			GameScenes.SPACECENTER,
			GameScenes.EDITOR,
			GameScenes.FLIGHT,
			GameScenes.TRACKSTATION,
		})
	]
	public class ModuleLoader
	{
		public static List<ConstructorInfo> LoadModules (Type ofType, Type[] param_types)
		{
			var constructor_list = new List<ConstructorInfo> ();
			foreach (var loaded in AssemblyLoader.loadedAssemblies) {
				var assembly = loaded.assembly;
				//Debug.Log (String.Format ("[KS] LoadModules {0}", loaded.name));
				var types = assembly.GetTypes ();
				for (int i = 0; i < types.Length; i++) {
					var type = types[i];
					if (type.GetInterfaces ().Contains (ofType)) {
						//Debug.Log (String.Format ("[KS] LoadModules type:{0}", type.Name));
						var constructor = type.GetConstructor (param_types);
						if (constructor != null) {
							Debug.Log (String.Format ("[KS] found module {0}",
													  type.Name));
							constructor_list.Add (constructor);
						}
					}
				}
			}
			return constructor_list;
		}
	}

}
