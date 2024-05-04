using GraphProcessor;
using TerrainMixture.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mixture
{
	[System.Serializable]
	public class AssetPreviewNode<T> : MixtureNode, ICreateNodeFrom<T>
		where T : Object, IHasPrototypePrefab, IHasPrototypeTexture
	{
		[ShowInInspector] public T template;

		public override Texture previewTexture => PreviewRT;

		// We don't use the 'Custom' part but we need a CRT for utility functions
		CustomRenderTexture PreviewRT;

		// Disable reset on output texture settings
		protected override bool CanResetPort(NodePort port) => false;

		public bool InitializeNodeFromObject(T value)
		{
			template = value;
			return true;
		}

		protected override void Enable()
		{
			base.Enable();
			UpdateTempRT();

			beforeProcessSetup += UpdateTempRT;
			afterProcessCleanup += UpdateTempRT;
		}

		void UpdateTempRT()
		{
			// Update the temp RT so users that overrides processNode don't have to do it
			UpdateTempRenderTexture(ref PreviewRT, hasMips: false,
				depthBuffer: false);

#if UNITY_EDITOR
			if (template != null)
			{
				if (!template.UsePrototypeTexture && template.PrototypePrefab != null)
				{
					Graphics.Blit(AssetPreview.GetAssetPreview(template.PrototypePrefab), PreviewRT);
				}

				if (template.UsePrototypeTexture && template.PrototypeTexture != null)
				{
					Graphics.Blit(template.PrototypeTexture, PreviewRT);
				}
			}
#endif
		}
	}
}
