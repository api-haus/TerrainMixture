using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMixture.Utils
{
	public static class ProgressUtility
	{
		public static void Report(int id, float progress, string description)
		{
#if UNITY_EDITOR
			Progress.Report(id, progress, description);
#endif
		}

		public static bool Exists(int progressId)
		{
#if UNITY_EDITOR
			return Progress.Exists(progressId);
#endif
			return true;
		}

		public static void Remove(int progressId)
		{
			if (!Exists(progressId))
			{
				return;
			}
#if UNITY_EDITOR
			Progress.Remove(progressId);
#endif
		}

		public static int Start(
			string name,
			string description = null,
			Progress.Options options = Progress.Options.None,
			int parentId = -1)
		{
#if UNITY_EDITOR
			return Progress.Start(name, description, options, parentId);
#endif
			return -1;
		}

		public static void RegisterCancelCallback(int progressId, Func<bool> abort)
		{
#if UNITY_EDITOR
			Progress.RegisterCancelCallback(progressId, abort);
#endif
		}

		public static void UnregisterCancelCallback(int progressId)
		{
			if (!Exists(progressId))
			{
				return;
			}
#if UNITY_EDITOR
			Progress.UnregisterCancelCallback(progressId);
#endif
		}
	}
}
