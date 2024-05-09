using GraphProcessor;
using Mixture;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainMixture.Runtime
{
	public static class MixtureExtensions
	{
		public static void RemoveParameterByName(this MixtureGraph graph, string name)
		{
			ExposedParameter par;
			while ((par = graph.GetExposedParameter(name)) != null)
			{
				graph.RemoveExposedParameter(par);
			}
		}

		public static void ForceReplaceParameter<T>(this MixtureGraph graph, string name, object value)
			where T : ExposedParameter, new()
		{
			graph.RemoveParameterByName(name);
			if (value != null)
			{
				var parameter = new T
				{
					name = name,
					input = true,
					value = value,
					settings = new ExposedParameter.Settings
					{
						isHidden = true,
						expanded = false
					}
				};

				graph.AddExposedParameter(parameter);
			}
#if UNITY_EDITOR
			EditorUtility.SetDirty(graph);
#endif
		}
	}
}
