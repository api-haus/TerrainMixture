namespace TerrainMixture.Utils
{
	using UnityEngine;

	public static class TextureUtility
	{
		public static Texture2D ToTexture2D(Texture source, TextureFormat format)
		{
			var tex2D = new Texture2D(source.width, source.height, format, false, true);
			Graphics.ConvertTexture(source, tex2D);

			return tex2D;
		}

		public static Texture2D SyncReadback(RenderTexture source, TextureFormat format, bool linear = false)
		{
			var ct = RenderTexture.active;
			RenderTexture.active = source;

			var tex2D = new Texture2D(source.width, source.height, format, false, linear);

			tex2D.ReadPixels(new Rect(0, 0, tex2D.width, tex2D.height), 0, 0, false);
			tex2D.Apply();

			RenderTexture.active = ct;

			return tex2D;
		}

		public static RenderTexture ToTemporaryRT(Texture from, int resolution, RenderTextureFormat format)
		{
			var rt = RenderTexture.GetTemporary(resolution, resolution, 0, format, RenderTextureReadWrite.sRGB);
			Graphics.Blit(from, rt);
			return rt;
		}
	}
}
