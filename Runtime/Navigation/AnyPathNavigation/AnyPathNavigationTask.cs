#if ANYPATH
using System.Collections;
using TerrainMixture.Tasks;
using UnityEngine;

namespace TerrainMixture.Runtime.Navigation.AnyPathNavigation
{
	public class AnyPathNavigationTask : NavigationTask
	{
		public AnyPathNavigationTask(Terrain terrain, TaskController taskController) : base(terrain, taskController)
		{
		}

		protected override IEnumerator Process()
		{
			throw new System.NotImplementedException();
		}

		protected override void OnPostProcess()
		{
			throw new System.NotImplementedException();
		}
	}
}
#endif
