using System;
using System.Collections;
using TerrainMixture.Tasks;
using UnityEngine;

namespace TerrainMixture.Runtime.Navigation
{
	public abstract class NavigationTask : ProgressiveTask
	{
		protected readonly Terrain Terrain;

		protected NavigationTask(Terrain terrain, TaskController taskController) : base(taskController)
		{
			Terrain = terrain;
		}

		public static NavigationTask MakeTask(Terrain terrain, TaskController ctrl, NavigationSupport support)
		{
			switch (support)
			{
#if UNITY_AI_NAVIGATION
				case NavigationSupport.UnityAINavigation:
					return new UnityAINavigation.UnityAINavigationTask(terrain, ctrl);
#endif
#if ANYPATH
				case NavigationSupport.AnyPath:
					return new AnyPathNavigation.AnyPathNavigationTask(terrain, ctrl);
#endif
				case NavigationSupport.None:
				default:
					throw new ArgumentOutOfRangeException($"{support} not supported");
			}
		}

		public static IEnumerator GenerateNavMesh(Terrain terrain, TaskController ctrl, NavigationSupport support)
		{
			if (support == NavigationSupport.None) yield break;
			var navigationTask = MakeTask(terrain, ctrl, support);

			yield return navigationTask.Start();
		}
	}
}
