using System;
using System.Collections;
using UnityEngine;

namespace TerrainMixture.Tasks
{
	public abstract class ProgressiveTask : IDisposable
	{
		protected bool IsCancelled =>
			TaskController is null or { IsCancelled: true };

		public readonly TaskController TaskController;

		protected ProgressiveTask(TaskController taskController)
		{
			TaskController = taskController;
		}

		protected abstract IEnumerator Process();

		protected abstract void OnAbort();

		public IEnumerator Start()
		{
			TaskController.AbortSignal += Dispose;

			yield return Process();
		}

		public void Dispose()
		{
			if (!IsCancelled)
			{
				TaskController.AbortSignal -= Dispose;
			}

			OnAbort();
		}
	}
}
