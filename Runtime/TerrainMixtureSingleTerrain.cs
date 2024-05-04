using System;
using System.Collections;
using GraphProcessor;
using Mixture;
using TerrainMixture.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	[ExecuteAlways]
	public class TerrainMixtureSingleTerrain : MonoBehaviour
	{
		const float DebounceTime = 4f;

		public Texture graphAsset;
		public Terrain terrain;

		[HideInInspector] [SerializeField] MixtureGraph graph;

		[SerializeField] bool isDirty;

		void OnValidate()
		{
			if (graphAsset != null && graphAsset)
			{
				graph = MixtureDatabase.GetGraphFromTexture(graphAsset);
			}
		}

#if UNITY_EDITOR
		void LateUpdate()
		{
			if (Application.isPlaying) return;
			if (isDirty)
			{
				EditorCoroutineUtility.StartCoroutine(DebounceUpdate(), this);

				isDirty = false;
			}
		}

		Guid Latest;
		IEnumerator DebounceUpdate()
		{
			var guid = Latest = Guid.NewGuid();

			yield return CoroutineUtility.WaitForSeconds(DebounceTime);

			if (Latest != guid)
			{
				yield break;
			}

			if (terrain != null && graph != null && terrain && graph)
			{
				TerrainMixtureRuntime.UpdateTerrain(terrain, graph);
			}
		}
#endif

		void OnEnable()
		{
			OnValidate();
			if (!graph)
			{
				return;
			}

			graph.onExposedParameterValueChanged += OnExposedParameterValueChanged;
			graph.onGraphChanges += OnGraphChanges;
		}

		void OnExposedParameterValueChanged(ExposedParameter obj)
		{
			isDirty = true;
		}

		void OnGraphChanges(GraphChanges obj)
		{
			isDirty = true;
		}

		void OnDisable()
		{
			if (!graph)
			{
				return;
			}

			graph.onExposedParameterValueChanged -= OnExposedParameterValueChanged;
			graph.onGraphChanges -= OnGraphChanges;
		}
	}
}
