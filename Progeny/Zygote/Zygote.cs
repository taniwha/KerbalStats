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
		public string mother_id;
		public string father_id;
		public ILocation location;
		protected GenePair[] genes;

		public string id
		{
			get;
			private set;
		}

		public Zygote (ProtoCrewMember kerbal)
		{
			mother_id = "";
			father_id = "";
			id = ProgenyScenario.current.NextZygoteID ();
			genes = Genome.GetGenes (kerbal);
		}

		public Zygote (Female mother, Male father)
		{
			mother_id = mother.id;
			father_id = father.id;
			id = ProgenyScenario.current.NextZygoteID ();
			genes = Genome.Combine (mother.genes, father.genes);
		}

		public Zygote (Zygote prevStage)
		{
			mother_id = prevStage.mother_id;
			father_id = prevStage.father_id;
			id = prevStage.id;
			genes = prevStage.genes;
		}

		public Zygote (ConfigNode node)
		{
			id = node.GetValue ("id");
			mother_id = node.GetValue ("mother");
			father_id = node.GetValue ("father");
			genes = Genome.ReadGenes (node);
		}

		public virtual void Save (ConfigNode node)
		{
			node.AddValue ("id", id);
			node.AddValue ("mother", mother_id);
			node.AddValue ("father", father_id);
			Genome.WriteGenes (genes, node);
		}
	}
}
