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
		const float DebounceTime = .33f;

		public Texture graphAsset;
		public Terrain terrain;

		[HideInInspector] [SerializeField] MixtureGraph graph;

		[SerializeField] bool isDirty;
		[SerializeField] bool watchUpdates;

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
			// if (Application.isPlaying) return;
			if (watchUpdates && isDirty)
			{
				EditorCoroutineUtility.StartCoroutine(DebounceUpdate(Guid.NewGuid()), this);

				isDirty = false;
			}
		}

		static Guid Latest;

		IEnumerator DebounceUpdate(Guid guid)
		{
			Latest = guid;

			yield return CoroutineUtility.WaitForSeconds(DebounceTime, true);

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
