using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalStats {
	public interface IKerbalExt
	{
		string name { get; }
		void AddKerbal (ProtoCrewMember kerbal);
		void RemoveKerbal (ProtoCrewMember kerbal);
		void Load (ProtoCrewMember kerbal, ConfigNode node);
		void Save (ProtoCrewMember kerbal, ConfigNode node);
	}
}
