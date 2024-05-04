using System;
using System.Collections;
using TerrainMixture.Utils;
using UnityEngine;

namespace TerrainMixture.Tasks
{
	public class TaskController : IDisposable
	{
		internal Action AbortSignal;

		internal int TaskId;
		internal bool IsCancelled { get; private set; } = false;
		internal bool IsCompleted { get; private set; } = false;

		public int TotalSteps;
		public int CurrentStep;

		public void Begin(string name)
		{
			TaskId = ProgressUtility.Start(name);

			ProgressUtility.RegisterCancelCallback(TaskId, CancelCallback);
		}

		public void NextStep(string description)
		{
			ProgressUtility.Report(TaskId, ++CurrentStep / (float)TotalSteps, description);
		}

		public void Progress(float progress, string description)
		{
			ProgressUtility.Report(TaskId, progress, description);
		}

		public void RelativeProgress(float relativeProgress, string description)
		{
			var progress = Mathf.Lerp(
				CurrentStep / (float)TotalSteps,
				(CurrentStep + 1) / (float)TotalSteps,
				relativeProgress
			);
			ProgressUtility.Report(TaskId, progress, description);
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
