using GraphProcessor;
using Mixture;
using TerrainMixture.Authoring;
using UnityEditor.UIElements;

namespace TerrainMixture.Editor
{
	[NodeCustomEditor(typeof(TerrainDetailsNode))]
	public class DetailOutputNodeView : MixtureNodeView
	{
		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			var objectField = new ObjectField("Detail Template")
			{
				objectType = typeof(DetailTemplate),
			};
			objectField.BindProperty(FindSerializedProperty("template"));
			controlsContainer.Add(objectField);
		}
	}
}