namespace TerrainMixture.Utils
{
	using UnityEngine;

	public static class TextureUtility
	{
		public static Texture2D SyncReadback(RenderTexture source, TextureFormat format = TextureFormat.ARGB32,
			bool linear = true)
		{
			var ct = RenderTexture.active;
			RenderTexture.active = source;

			var tex2D = new Texture2D(source.width, source.height, format, false, linear);

			tex2D.ReadPixels(new Rect(0, 0, tex2D.width, tex2D.height), 0, 0, false);
			tex2D.Apply();

			RenderTexture.active = ct;

			return tex2D;
		}

		public static RenderTexture CopyRT(RenderTexture from)
		{
			return CopyRT(from, from.format);
		}

		public static RenderTexture CopyRT(RenderTexture from, RenderTextureFormat format)
		{
			return CopyRT(from, format, from.width);
		}

		public static RenderTexture CopyRT(RenderTexture from, RenderTextureFormat format, int size)
		{
			var desc = from.descriptor;
			desc.colorFormat = format;
			desc.width = size;
			desc.height = size;
			var rt = RenderTexture.GetTemporary(desc);
			Graphics.Blit(from, rt);
			return rt;
		}

		public static RenderTexture CopyRTReadWrite(RenderTexture from)
		{
			return ComputeShaderTextureCopy.CopyAsReadWrite(from);
		}
	}
}
