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
using System.Linq;

namespace KerbalStats.Genome {

	public class Courage : Trait
	{
		static float pdf0 (float x)
		{
			return (float) ((Math.Exp (1 - x) - 1) / (Math.E - 2));
		}
		static float cdf0 (float x)
		{
			return (float) ((Math.E - Math.Exp (1 - x) - x) / (Math.E - 2));
		}

		static float pdf12 (float x)
		{
			return (float) ((3*Math.E - Math.Exp (x) - Math.Exp (1 - x) - 4) / (Math.E - 2));
		}
		static float cdf12 (float x)
		{
			return (float) (((3 * Math.E - 4) * x - Math.Exp (x) + Math.Exp (1 - x) + 1 - Math.E) / (Math.E - 2));
		}

		static float pdf3 (float x)
		{
			return (float) ((Math.Exp (x) - 1) / (Math.E - 2));
		}
		static float cdf3 (float x)
		{
			return (float) ((Math.Exp (x) - x - 1) / (Math.E - 2));
		}

		static ContinuousDistribution[] distributions = {
			new ContinuousDistribution (pdf0,  0, 1, cdf0),
			new ContinuousDistribution (pdf12, 0, 1, cdf12),
			new ContinuousDistribution (pdf3,  0, 1, cdf3),
		};

		static uint[][][] genes = {
			new uint[][]{
				new uint[] {0, 0},
				new uint[] {0, 1},
				new uint[] {1, 1},
			},
			new uint[][]{
				new uint[] {0, 0},
				new uint[] {1, 0},
				new uint[] {1, 1},
			},
		};

		public string name
		{
			get {
				return "Courage";
			}
		}

		public int GeneSize
		{
			get {
				return 1;
			}
		}

		ContinuousDistribution ChooseDistribution (GenePair gene)
		{
			uint index = (gene.a & 1) + (gene.b & 1);
			return distributions[index];
		}

		public GenePair CreateGene (float courage)
		{
			float[] probabilities = new float[distributions.Length];
			for (int i = 0; i < distributions.Length; i++) {
				probabilities[i] = distributions[i].Density (courage);
			}
			//for (int i = 0; i < probabilities.Length; i++) {
			//	Console.WriteLine(String.Format ("{0} {1}", i, probabilities[i]));
			//}
			var dist = new DiscreteDistribution (probabilities);
			int index = dist.Value (UnityEngine.Random.Range (0, 1f));
			var gene = genes[UnityEngine.Random.Range (0, 2)][index];
			return new GenePair (this, gene[0], gene[1]);
		}

		public GenePair CreateGene (string courageS)
		{
			float courage = 0;
			float.TryParse (courageS, out courage);
			return CreateGene (courage);
		}

		public GenePair CreateGene (ProtoCrewMember pcm)
		{
			if (pcm == null) {
				var c = UnityEngine.Random.Range (0, 1f);
				return CreateGene (c);
			}
			var gene = Genome.Prefab (this, pcm);
			if (gene == null) {
				gene = CreateGene (pcm.courage);
			}
			return gene;
		}

		public string CreateValue (GenePair gene)
		{
			ContinuousDistribution dist = ChooseDistribution (gene);
			return dist.Value (UnityEngine.Random.Range (0, 1f)).ToString ("G9");
		}
	}
}
