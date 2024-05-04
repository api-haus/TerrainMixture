using System;
using System.Collections.Generic;
using GraphProcessor;
using TerrainMixture.Authoring;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using TerrainMixture.Utils;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TreeInstanceNative
	{
		public static readonly int Stride = UnsafeUtility.SizeOf<TreeInstanceNative>();

		public Vector3 position;
		public float rotation;
		public Vector2 scale;
		public uint id;
		public bool IsCreated => id != 0;
	}

	[Documentation(@"
Sample tree positions from density texture.
")]
	[System.Serializable]
	[NodeMenuItem("Terrain/Trees Output")]
	public class TerrainTreesNode : ComputeShaderNode, ICreateNodeFrom<TreeTemplate>
	{
		public override bool canProcess => template != null && template.Validate() && densityMask != null;

		[Input] public Texture densityMask;

		[Input] public int seed = 0;

		public TreeTemplate template;

		[Output] public ComputeBuffer resultingPositions;

		[FormerlySerializedAs("maxSplatCount")] [ShowInInspector] [SerializeField]
		private int maxInstanceCount = 256;

		// Grid
		[Range(0, 2)] public float lambda = 0;

		// Other
		[ShowInInspector] public Vector3 positionJitter = Vector3.zero;

		// Rotation
		public float minAngle = 0;

		public float maxAngle = 360;

		// Scale
		[ShowInInspector] public Vector2 minScale = new Vector2(0.75f, 0.75f);

		[ShowInInspector] public Vector2 maxScale = new Vector2(1.5f, 2f);

		public override string name => "Terrain Trees Output";
		protected override string computeShaderResourcePath => "TerrainMixture/TreeSplatter";

		public override bool showDefaultInspector => true;

		public ComputeBuffer TreeInstancesBuffer;

		int GeneratePointKernel;

		static int LessOrEqualPot(int n)
		{
			int res = 0;
			for (int i = n; i >= 1; i--)
			{
				// If i is a power of 2
				if ((i & (i - 1)) == 0)
				{
					res = i;
					break;
				}
			}

			return res;
		}

		public int SafeMaxSplatCount =>
			Mathf.Max(
				8,
				Mathf.Min(LessOrEqualPot((int)(SystemInfo.maxGraphicsBufferSize / TreeInstanceNative.Stride / 2)),
					Mathf.ClosestPowerOfTwo(maxInstanceCount)));

		public int LiveInstancesCount =>
			TreeInstancesBuffer != null && TreeInstancesBuffer.IsValid()
				? TreeInstancesBuffer.count
				: 0;

		// ReSharper disable InconsistentNaming
		static readonly int _TreeInstances = Shader.PropertyToID("_TreeInstances");
		static readonly int _DensityMask = Shader.PropertyToID("_DensityMask");
		static readonly int _DensityMaskResolution = Shader.PropertyToID("_DensityMaskResolution");
		static readonly int _HeightMap = Shader.PropertyToID("_HeightMap");
		static readonly int _HeightMapResolution = Shader.PropertyToID("_HeightMapResolution");
		static readonly int _TerrainDimensions = Shader.PropertyToID("_TerrainDimensions");
		static readonly int _TerrainHeight = Shader.PropertyToID("_TerrainHeight");
		static readonly int _Seed = Shader.PropertyToID("_Seed");
		static readonly int _Lambda = Shader.PropertyToID("_Lambda");
		static readonly int _MinAngle = Shader.PropertyToID("_MinAngle");
		static readonly int _MaxAngle = Shader.PropertyToID("_MaxAngle");
		static readonly int _MinScale = Shader.PropertyToID("_MinScale");
		static readonly int _MaxScale = Shader.PropertyToID("_MaxScale");
		static readonly int _PositionJitter = Shader.PropertyToID("_PositionJitter");
		static readonly int _Time = Shader.PropertyToID("_Time");
		static readonly int _ElementCount = Shader.PropertyToID("_ElementCount");
		// ReSharper restore InconsistentNaming

		Texture2D CachedHeightmap;

		protected override void Enable()
		{
			Disable();

			// Debug.Log("0. T.Enable()");

			base.Enable();

			Allocate();

			GeneratePointKernel = computeShader.FindKernel("GenerateSplatPoints");

			beforeProcessSetup += UpdatePreview;
			afterProcessCleanup += UpdatePreview;
		}

		void Allocate()
		{
			if (TreeInstancesBuffer != null)
			{
				TreeInstancesBuffer.Dispose();
				TreeInstancesBuffer = null;
			}

			TreeInstancesBuffer = new ComputeBuffer(
				SafeMaxSplatCount,
				TreeInstanceNative.Stride,
				ComputeBufferType.Structured,
				ComputeBufferMode.Dynamic);
		}

		void UpdatePreview()
		{
#if UNITY_EDITOR
			if (template && template.PrototypePrefab)
			{
				var preview = AssetPreview.GetAssetPreview(template.PrototypePrefab);
				Graphics.Blit(preview, tempRenderTexture);
			}
#endif
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
			{
				return false;
			}

			if (!SetComputeArgs(cmd, SafeMaxSplatCount))
			{
				return false;
			}

			computeShader.GetKernelThreadGroupSizes(GeneratePointKernel, out uint x, out _, out _);
			DispatchCompute(cmd, GeneratePointKernel, SafeMaxSplatCount + ((int)x - SafeMaxSplatCount % (int)x));

			// Debug.Log("1. T.Dispatch()");
			resultingPositions = TreeInstancesBuffer;

			return true;
		}

		bool SetComputeArgs(CommandBuffer cmd, int safeMaxSplatCount)
		{
			var terrainDimensions = graph.GetParameterValue<float>("Terrain Dimensions");
			var terrainHeight = graph.GetParameterValue<float>("Terrain Height");

			var heightOutputNode = graph.graphOutputs.OfType<TerrainHeightOutputNode>().FirstOrDefault();
			if (null == heightOutputNode)
			{
				Debug.LogWarning("Invalid height output");
				return false;
			}

			var heightMap = heightOutputNode.heightOutput;
			if (null == heightMap)
			{
				Debug.LogWarning("Invalid height texture");
				return false;
			}

			if (CachedHeightmap)
			{
				ObjectUtility.Destroy(CachedHeightmap);
				CachedHeightmap = null;
			}

			// if (TreeInstancesBuffer == null)
			// {
			// 	Allocate();
			// 	// IDK How this happens, lets just replace it anyway.
			// }

			if (TreeInstancesBuffer == null || !TreeInstancesBuffer.IsValid())
			{
				// Debug.LogWarning("Invalid tree buffer");
				Allocate();
				// return false;
			}

			CachedHeightmap = TextureUtility.SyncReadback(heightMap, TextureFormat.R16, false);
			// AssetDatabase.CreateAsset(CachedHeightmap, $"Assets/_cachedHeightmap{DateTime.Now.Ticks}.asset");

			cmd.SetComputeTextureParam(computeShader, GeneratePointKernel, _DensityMask, densityMask);
			cmd.SetComputeIntParam(computeShader, _DensityMaskResolution, densityMask.width);
			cmd.SetComputeTextureParam(computeShader, GeneratePointKernel, _HeightMap, CachedHeightmap);
			cmd.SetComputeIntParam(computeShader, _HeightMapResolution, CachedHeightmap.width);

			cmd.SetComputeBufferParam(computeShader, GeneratePointKernel, _TreeInstances, TreeInstancesBuffer);
			cmd.SetComputeFloatParam(computeShader, _Time, (Application.isPlaying) ? Time.time : Time.realtimeSinceStartup);
			cmd.SetComputeFloatParam(computeShader, _ElementCount, safeMaxSplatCount);
			cmd.SetComputeFloatParam(computeShader, _MinAngle, minAngle * Mathf.Deg2Rad);
			cmd.SetComputeFloatParam(computeShader, _MaxAngle, maxAngle * Mathf.Deg2Rad);
			cmd.SetComputeFloatParam(computeShader, _Lambda, lambda);
			cmd.SetComputeVectorParam(computeShader, _MinScale, minScale);
			cmd.SetComputeVectorParam(computeShader, _MaxScale, maxScale);
			cmd.SetComputeVectorParam(computeShader, _PositionJitter, positionJitter);
			cmd.SetComputeFloatParam(computeShader, _Seed, seed);
			cmd.SetComputeFloatParam(computeShader, _TerrainDimensions, terrainDimensions);
			cmd.SetComputeFloatParam(computeShader, _TerrainHeight, terrainHeight);

			return true;
		}

		// NOTE: Disposed later after trees are consumed.
		// See TerrainTreeStream.cs
		protected override void Disable()
		{
			// Debug.Log("2. T.Disable()");
			TreeInstancesBuffer?.Dispose();
			ObjectUtility.Destroy(CachedHeightmap);
			base.Disable();
		}

		public bool InitializeNodeFromObject(TreeTemplate value)
		{
			template = value;
			return true;
		}

		public TreePrototype ToTreePrototype()
		{
			return new TreePrototype
			{
				prefab = template.prefab,
				bendFactor = template.bendFactor,
				navMeshLod = template.navMeshLod,
			};
		}
	}
}
