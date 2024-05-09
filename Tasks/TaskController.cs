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
		public bool IsDisposed { get; private set; } = false;
		internal bool IsCompleted { get; private set; } = false;

		public int TotalSteps;
		public int CurrentStep;

		float LastProgress;

		public void Begin(string name)
		{
			TaskId = ProgressUtility.Start(name);

			ProgressUtility.RegisterCancelCallback(TaskId, CancelCallback);
		}

		public void NextStep(string description)
		{
			Report(++CurrentStep / (float)TotalSteps, description);
		}

		public void Progress(float progress, string description)
		{
			Report(progress, description);
		}

		public void RelativeProgress(float relativeProgress, string description)
		{
			var progress = Mathf.Lerp(
				CurrentStep / (float)TotalSteps,
				(CurrentStep + 1) / (float)TotalSteps,
				relativeProgress
			);
			Report(progress, description);
		}

		void Report(float progress, string description)
		{
			LastProgress = progress;
			ProgressUtility.Report(TaskId, progress, description);
		}

		public void Describe(string description)
		{
			ProgressUtility.Report(TaskId, LastProgress, description);
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
			IsDisposed = true;
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
			IsDisposed = true;
		}
	}
}
