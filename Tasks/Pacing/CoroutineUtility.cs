using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace TerrainMixture.Tasks.Pacing
{
	public static class CoroutineUtility
	{
		public static bool FrameSkip(ref float time, int maxFrameSkip = -1)
		{
			if (maxFrameSkip == -1)
			{
				maxFrameSkip = PacingController.CurrentMaxFrameSkip;
			}

			var sinceLastFrameSkip = Time.realtimeSinceStartup - time;
			var isLate = sinceLastFrameSkip >= maxFrameSkip * Time.fixedDeltaTime;

			if (isLate)
			{
				time = Time.realtimeSinceStartup;
			}

			return isLate;
		}

		public static IEnumerator WaitForSeconds(float time, bool forceEditorCoroutine = false)
		{
#if UNITY_EDITOR
			if (forceEditorCoroutine)
			{
				yield return new EditorWaitForSeconds(time);
				yield break;
			}
#endif
			yield return new WaitForSeconds(time);
		}

		public static void StartCoroutine(IEnumerator coroutine)
		{
#if UNITY_EDITOR
			EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
#else
			CoroutineSan.StartCoroutineOwnerless(coroutine);
#endif
		}
	}
}
