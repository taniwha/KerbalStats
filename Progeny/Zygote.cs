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
	using Genome;
	public class Zygote
	{
		// This is an abstraction for all stages of kerbal development, from
		// conception to death.
		Female mother;
		Male father;
		GenePair[] genes;

		public string id
		{
			get;
			private set;
		}

		public Zygote (Female mother, Male father)
		{
			this.mother = mother;
			this.father = father;
			id = ProgenyScenario.current.NextZygoteID ();
			genes = Genome.Combine (mother.kerbal, father.kerbal);
		}

		public Zygote (ConfigNode node)
		{
			id = node.GetValue ("id");
			string name;
			name = node.GetValue ("mother");
			mother = ProgenyTracker.instance[name] as Female;
			name = node.GetValue ("father");
			father = ProgenyTracker.instance[name] as Male;
			genes = Genome.ReadGenes (node);
		}

		public virtual void Save (ConfigNode node)
		{
			node.AddValue ("id", id);
			node.AddValue ("mother", mother.name);
			node.AddValue ("father", father.name);
			Genome.WriteGenes (genes, node);
		}
	}
}
