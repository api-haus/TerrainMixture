using System;
using System.Collections;

namespace TerrainMixture.Tasks
{
	public abstract class ProgressiveTask : IDisposable
	{
		protected bool IsCancelled =>
			TaskController is null or { IsDisposed: true };

		public readonly TaskController TaskController;

		protected ProgressiveTask(TaskController taskController)
		{
			TaskController = taskController;
		}

		protected abstract IEnumerator Process();

		protected abstract void OnPostProcess();

		public IEnumerator Start()
		{
			TaskController.AbortSignal += Dispose;

			yield return Process();

			TaskController.Complete();
		}

		public void Dispose()
		{
			TaskController.Complete();
			if (!IsCancelled)
			{
				TaskController.AbortSignal -= Dispose;
			}

			OnPostProcess();
		}
	}
}
