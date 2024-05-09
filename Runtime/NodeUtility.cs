using System.Collections.Generic;
using System.Linq;
using GraphProcessor;

namespace TerrainMixture.Runtime
{
	public static class NodeUtility
	{
		public static IEnumerable<BaseNode> GetRootDependencies(this BaseNode node)
		{
			var rootDependencies = new HashSet<BaseNode>();
			var inputNodes = new Stack<BaseNode>(node.GetInputNodes());

			rootDependencies.Add(node);

			while (inputNodes.Count > 0)
			{
				var child = inputNodes.Pop();
				var childInputs = child.GetInputNodes().ToArray();

				if (childInputs.Length == 0 && !rootDependencies.Add(child))
					continue;

				foreach (var parent in childInputs)
					inputNodes.Push(parent);

				// Max dependencies on a node, maybe we can put a warning if it's reached?
				if (rootDependencies.Count > 2000)
					break;
			}

			return rootDependencies.OrderBy(d => d.computeOrder);
		}

		public static IEnumerable<BaseNode> GetAllDependencies(this BaseNode node)
		{
			var dependencies = new HashSet<BaseNode>();
			var inputNodes = new Stack<BaseNode>(node.GetInputNodes());

			dependencies.Add(node);

			while (inputNodes.Count > 0)
			{
				var child = inputNodes.Pop();

				if (!dependencies.Add(child))
					continue;

				foreach (var parent in child.GetInputNodes())
					inputNodes.Push(parent);

				// Max dependencies on a node, maybe we can put a warning if it's reached?
				if (dependencies.Count > 2000)
					break;
			}

			return dependencies.OrderBy(d => d.computeOrder);
		}
	}
}
