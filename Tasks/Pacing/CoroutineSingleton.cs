using System.Collections;
using UnityEngine;

namespace TerrainMixture.Tasks.Pacing
{
	public class CoroutineSingleton : MonoBehaviour
	{
		static CoroutineSingleton Instance;
		static bool IsCreated => Instance != null && Instance;

		void Awake()
		{
			if (IsCreated && Instance != this) { Destroy(this); }
			else { Instance = this; }
		}

		public static void StartCoroutineOwnerless(IEnumerator coroutine)
		{
			if (Instance == null)
			{
				Instance = new GameObject { hideFlags = HideFlags.HideAndDontSave }.AddComponent<CoroutineSingleton>();
			}

			Instance.StartCoroutine(coroutine);
		}
	}
}
