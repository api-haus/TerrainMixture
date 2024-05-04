using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMixture.Utils
{
	public static class ProgressUtility
	{
		public static bool Validate(int id) => id != 0 && Exists(id);

		public static void Report(int id, float progress, string description)
		{
			if (!Validate(id))
				return;
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

		public static void Remove(int id)
		{
			if (!Validate(id))
				return;
#if UNITY_EDITOR
			Progress.Remove(id);
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

		public static void RegisterCancelCallback(int id, Func<bool> abort)
		{
			if (!Validate(id))
				return;
#if UNITY_EDITOR
			Progress.RegisterCancelCallback(id, abort);
#endif
		}

		public static void UnregisterCancelCallback(int id)
		{
			if (!Validate(id))
				return;
#if UNITY_EDITOR
			Progress.UnregisterCancelCallback(id);
#endif
		}
	}
}
