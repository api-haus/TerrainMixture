using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainMixture.Utils
{
	public static class ComputeShaderTextureCopy
	{
		static readonly ComputeShader CopyTexture;
		static readonly int KCopyRT;
		static readonly int3 ThreadGroupSize;

		public static void DispatchComputeFor(this CommandBuffer cmd, ComputeShader cs, int kernelIndex,
			int3 threadGroupSize, int3 dispatchSize)
		{
			var finalSize = math.max(dispatchSize / threadGroupSize, 1);
			cmd.DispatchCompute(cs, kernelIndex, finalSize.x, finalSize.y, finalSize.z);
		}

		static ComputeShaderTextureCopy()
		{
			CopyTexture = Resources.Load<ComputeShader>("TerrainMixture/CopyRT");
			KCopyRT = CopyTexture.FindKernel("CopyRT");
			CopyTexture.GetKernelThreadGroupSizes(KCopyRT, out var x, out var y, out var z);
			ThreadGroupSize = (int3)new uint3(x, y, z);
		}

		public static RenderTexture CopyAsReadWrite(RenderTexture original)
		{
			var desc = original.descriptor;
			desc.enableRandomWrite = true;
			var destination = new RenderTexture(desc);
			destination.Create();

			var cmd = CommandBufferPool.Get();
			// Set up profiling scope for Profiler & Frame Debugger
			using (new ProfilingScope(cmd, new ProfilingSampler("TerrainMixture.CopyRT")))
			{
				cmd.SetComputeTextureParam(CopyTexture, KCopyRT, "_Source", original);
				cmd.SetComputeTextureParam(CopyTexture, KCopyRT, "_Destination", destination);
				cmd.DispatchComputeFor(CopyTexture, KCopyRT, ThreadGroupSize, new int3(original.width, original.height, 1));
			}

			Graphics.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

			return destination;
		}
	}
}
