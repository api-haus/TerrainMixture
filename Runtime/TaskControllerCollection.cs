using System.Collections.Generic;
using TerrainMixture.Tasks;

namespace TerrainMixture.Runtime
{
	static class TaskControllerCollection
	{
		// simply limit one controller per tile for sanity's sake... until domain reload.
		static readonly Dictionary<int, TaskController> ControllerByTerrainId = new();

		internal static bool PopController(int id, out TaskController ctrl)
		{
			var exists = ControllerByTerrainId.TryGetValue(id, out ctrl);

			if (exists)
			{
				ControllerByTerrainId.Remove(id);
			}

			return exists;
		}

		internal static TaskController CreateController(int id)
		{
			var ctrl = new TaskController();
			ControllerByTerrainId.Add(id, ctrl);
			return ctrl;
		}
	}
}
