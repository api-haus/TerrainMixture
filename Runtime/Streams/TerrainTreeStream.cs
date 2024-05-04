using System;
using System.Collections;
using Mixture;
using TerrainMixture.Tasks;
using TerrainMixture.Utils;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMixture.Runtime.Streams
{
	public class TerrainTreeStream : ProgressiveTask
	{
		const bool DoSyncReadbacks = true;
		const bool SnapToHeightmap = false;

		readonly int MaxFrameSkip = 4;
		readonly TerrainData TerrainData;
		readonly ComputeBuffer SourceBuffer;
		readonly int Layer;
		readonly int MaxPoints;

		public TerrainTreeStream(TaskController taskController, TerrainData terrainData, ComputeBuffer sourceBuffer,
			int maxPoints, int treeLayer) : base(taskController)
		{
			TerrainData = terrainData;
			SourceBuffer = sourceBuffer;
			MaxPoints = maxPoints;
			Layer = treeLayer;
		}

		protected override IEnumerator Process()
		{
			if (SourceBuffer == null || !SourceBuffer.IsValid()) yield break;

			// Debug.Log("TS.Capture()");
			TreeInstanceNative[] splatPoints;
			if (DoSyncReadbacks && !Application.isPlaying)
			{
				// ("questionably") helps avoid loosing resource during domain reload when developing/debugging the package.
				splatPoints = new TreeInstanceNative[MaxPoints];
				SourceBuffer.GetData(splatPoints);
			}
			else
			{
				var readbackRequest =
					AsyncGPUReadback.Request(SourceBuffer, TreeInstanceNative.Stride * MaxPoints, 0);
				while (!readbackRequest.done)
				{
					if (IsCancelled)
					{
						TaskController.Complete();
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

			var treeInstances = new TreeInstance[Mathf.Min(MaxPoints, splatPoints.Length)];

			var time = Time.realtimeSinceStartup;
			var total = treeInstances.Length;
			var current = 0;

			for (var i = 0; i < treeInstances.Length; i++)
			{
				var splat = splatPoints[i];
				if (!splat.IsCreated) continue;

				// Debug.Log($"p#{splat.id}: {splat.position}, {splat.scale}, {splat.rotation}");

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
				var relativeProgress = ++current / (float)total;

				if (CoroutineUtility.FrameSkip(ref time, MaxFrameSkip))
				{
					if (IsCancelled)
					{
						TaskController.Complete();
						yield break;
					}

					TaskController.RelativeProgress(relativeProgress, "Trees...");

					yield return null;
				}
			}

#if UNITY_2019_1_OR_NEWER
			TerrainData.SetTreeInstances(treeInstances, SnapToHeightmap);
#else
      TerrainData.treeInstances = treeInstances;
#endif

#if UNITY_EDITOR
			EditorUtility.SetDirty(TerrainData);
#endif
		}

		protected override void OnEndProcess()
		{
			// Debug.Log("TS.OnEndProcess()");
			SourceBuffer?.Dispose();
		}
	}
}