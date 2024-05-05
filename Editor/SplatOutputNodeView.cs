using GraphProcessor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Mixture;

namespace TerrainMixture.Editor
{
	[NodeCustomEditor(typeof(TerrainSplatOutputNode))]
	public class SplatOutputNodeView : MixtureNodeView
	{
		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			var sliderField = new SliderInt("Splat Index", 0, 3, SliderDirection.Horizontal, 1);
			sliderField.showInputField = true;
			sliderField.BindProperty(FindSerializedProperty("splatIndex"));
			controlsContainer.Add(sliderField);

			var layersField = new PropertyField(FindSerializedProperty("terrainLayers"));
			controlsContainer.Add(layersField);
		}
	}
}
