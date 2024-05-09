using System;
using System.Collections;
using Mixture.Nodes;
using TerrainMixture.Tasks;
using TerrainMixture.Tasks.Pacing;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMixture.Runtime.Streams
{
	public class TerrainTreeStream : ProgressiveTask
	{
		// Not possible to change until we refactor how the buffer disposed on tree output node
		const bool DoSyncReadbacks = false;
		const bool SnapToHeightmap = true;

		readonly TerrainData TerrainData;
		readonly ComputeBuffer SourceBuffer;
		readonly int Layer;
		readonly int SelectedPoints;

		public TerrainTreeStream(TaskController taskController, TerrainData terrainData, ComputeBuffer sourceBuffer,
			int selectedPoints, int treeLayer) : base(taskController)
		{
			TerrainData = terrainData;
			SourceBuffer = sourceBuffer;
			SelectedPoints = selectedPoints;
			Layer = treeLayer;
		}

		protected override IEnumerator Process()
		{
			if (SourceBuffer == null || !SourceBuffer.IsValid() || SourceBuffer.count < SelectedPoints) yield break;

			TreeInstanceNative[] splatPoints;
			if (DoSyncReadbacks)
			{
				splatPoints = new TreeInstanceNative[SelectedPoints];
				SourceBuffer.GetData(splatPoints);
			}
			else
			{
				var readbackRequest =
					AsyncGPUReadback.Request(SourceBuffer, TreeInstanceNative.Stride * SelectedPoints, 0);
				while (!readbackRequest.done)
				{
					if (IsCancelled)
					{
						yield break;
					}

					yield return null;
				}

				if (readbackRequest.hasError)
				{
					throw new Exception("Async readback has error");
				}

				splatPoints = readbackRequest.GetData<TreeInstanceNative>().ToArray();
			}

			var treeInstances = new TreeInstance[Mathf.Min(SelectedPoints, splatPoints.Length)];

			var time = Time.realtimeSinceStartup;
			var total = treeInstances.Length;
			var current = 0;

			var valid = 0;

			for (var i = 0; i < treeInstances.Length; i++)
			{
				var relativeProgress = ++current / (float)total;
				var splat = splatPoints[i];
				if (!splat.IsCreated) continue;

				valid++;
				treeInstances[i] = new TreeInstance
				{
					position = splat.position,
					widthScale = splat.scale.x,
					heightScale = splat.scale.y,
					rotation = splat.rotation,
					color = Color.white,
					lightmapColor = Color.white,
					prototypeIndex = Layer,
				};

				if (CoroutineUtility.FrameSkip(ref time))
				{
					if (IsCancelled)
					{
						yield break;
					}

					TaskController.RelativeProgress(relativeProgress, "Trees...");

					yield return null;
				}
			}

			// Debug.Log($"trees:{valid}/{current}");
#if UNITY_2019_1_OR_NEWER
			TerrainData.SetTreeInstances(treeInstances, SnapToHeightmap);
#else
      TerrainData.treeInstances = treeInstances;
#endif

#if UNITY_EDITOR
			EditorUtility.SetDirty(TerrainData);
#endif
		}

		protected override void OnPostProcess()
		{
			if (SourceBuffer != null && SourceBuffer.IsValid())
			{
				SourceBuffer.Dispose();
			}
		}
	}
}
