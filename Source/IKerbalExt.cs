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
using UnityEngine;

namespace KerbalStats {
	/** Interface for a module that manages extended kerbal stats
	 *
	 * Note that the class implementing IKerbalExt must provide a
	 * constructor that takes a single KerbalStats parameter.
	 */
	public interface IKerbalExt
	{
		/** The name of the module.
		 *
		 * Used for indexing the available modules and also for checking
		 * nodes and values in the KerbalExt config node.
		 */
		string ModuleName { get; }
		/** Called when a kerbal is created.
		 *
		 * The module is expected to add a stats object (any type) and
		 * maintain it as necessary.
		 *
		 * \param kerbal    The extended stats for the new kerbal.
		 */
		void AddKerbal (KerbalExt kerbal);
		/** Called when a kerbal is removed.
		 *
		 * The module is given the opportunity to clean up any internal
		 * state that refers to the removed kerbal.
		 *
		 * \param kerbal    The extended stats for the kerbal being
		 *                  removed.
		 */
		void RemoveKerbal (KerbalExt kerbal);
		/** Called when a kerbal is loaded from persistence.
		 *
		 * \param kerbal    The Extended stats for the kerbal being
		 *                  loaded.
		 * \param node      The entire KerbalExt config node holding the
		 *                  saved stats. Shared between modules. The
		 *                  module is expected to use ModuleName for the
		 *                  value (simple stat) or node (complex stats).
		 */
		void Load (KerbalExt kerbal, ConfigNode node);
		/** Called when a kerbal is saved to persistence.
		 *
		 * \param kerbal    The Extended stats for the kerbal being
		 *                  saved.
		 * \param node      The entire KerbalExt config node holding the
		 *                  saved stats. Shared between modules. The
		 *                  module is expected to use ModuleName for the
		 *                  value (simple stat) or node (complex stats).
		 */
		void Save (KerbalExt kerbal, ConfigNode node);
		/** Called only when loading from an ancient save.
		 *
		 * Gives the module a chance to clear out its database when
		 * loading from pre-KSP-1.2 saves. Effectively obsolete.
		 */
		void Clear ();
		/** Called when KerbalStats is destroyed.
		 *
		 * Gives the module an opportunity to clean up any external
		 * resources or hooked GameEvents.
		 */
		void Shutdown ();

		/** Module implementation of the KerbalExt API.
		 *
		 * \param kerbal    The Extended stats for the kerbal being
		 *                  queried.
		 * \param parms     The &laquo;module-params&raquo; part of the
		 *                  query string. See
		 *                  ModName.KerbalStats.KerbalExt.Get() for
		 *                  details.
		 */
		string Get (KerbalExt kerbal, string parms);
	}
}
