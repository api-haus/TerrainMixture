using System;
using System.Collections;
using TerrainMixture.Utils;

namespace TerrainMixture.Tasks
{
	public class TaskController : IDisposable
	{
		internal Action AbortSignal;

		internal int TaskId;
		internal bool IsCancelled { get; private set; } = false;
		internal bool IsCompleted { get; private set; } = false;

		public void Begin(string name)
		{
			TaskId = ProgressUtility.Start(name);

			ProgressUtility.RegisterCancelCallback(TaskId, CancelCallback);
		}

		public void Progress(float progress, string description)
		{
			if (TaskId != 0)
			{
				ProgressUtility.Report(TaskId, progress, description);
			}
		}

		public void Complete()
		{
			IsCompleted = true;
		}

		public IEnumerator Wait()
		{
			while (!IsCompleted)
			{
				yield return null;
			}
		}

		public void Cancel()
		{
			IsCancelled = true;
			Dispose();
		}

		bool CancelCallback()
		{
			Cancel();
			return true;
		}

		public void Dispose()
		{
			if (TaskId != 0)
			{
				ProgressUtility.UnregisterCancelCallback(TaskId);
				ProgressUtility.Remove(TaskId);
				TaskId = 0;
			}

			AbortSignal?.Invoke();
			IsCancelled = true;
		}
	}
}
