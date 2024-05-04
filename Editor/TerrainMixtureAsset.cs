using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Mixture;

namespace TerrainMixture.Editor
{
	public abstract class TerrainMixtureAsset
	{
		static Texture2D Icon;

		static Texture2D icon =>
			Icon == null ? Icon = Resources.Load<Texture2D>("Icons/TerrainMixture_128") : Icon;

		static readonly string Extension = "asset";

		[MenuItem("Assets/Create/🧪Terrain Mixture🏔/⛰️Terrain Mixture Graph", false, 83)]
		public static void CreateStaticMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance<TerrainMixtureGraphAction>();
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
				$"New Terrain Mixture Graph.{Extension}", icon, null);
		}

		class TerrainMixtureGraphAction : MixtureGraphAction
		{
			public static readonly string mixtureEditorResourcesPath = "Packages/api.haus.terrainmixture/Editor/Resources/";

			public static readonly string template =
				$"{mixtureEditorResourcesPath}Templates/TerrainMixtureGraphTemplate.asset";

			// By default isRealtime is false so we don't need to initialize it like in the realtime mixture create function
			public override MixtureGraph CreateMixtureGraphAsset()
			{
				var g = MixtureEditorUtils.GetGraphAtPath(template);
				g = ScriptableObject.Instantiate(g) as MixtureGraph;

				g.ClearObjectReferences();

				foreach (var node in g.nodes)
				{
					// Duplicate all the materials from the template
					if (node is ShaderNode s && s.material != null)
					{
						s.material = new Material(s.material);
						s.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					}
					else if (node is OutputNode outputNode)
					{
						foreach (var outputSettings in outputNode.outputTextureSettings)
						{
							outputSettings.finalCopyMaterial = new Material(outputSettings.finalCopyMaterial);
							outputSettings.finalCopyMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
						}
					}
				}

				return g;
			}
		}

		abstract class MixtureGraphAction : EndNameEditAction
		{
			public abstract MixtureGraph CreateMixtureGraphAsset();

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var mixture = CreateMixtureGraphAsset();
				mixture.name = Path.GetFileNameWithoutExtension(pathName);
				mixture.hideFlags = HideFlags.HideInHierarchy;

				AssetDatabase.CreateAsset(mixture, pathName);

				// Generate the output texture:
				mixture.outputTextures.Clear();
				if (mixture.type == MixtureGraphType.Realtime)
				{
					mixture.UpdateRealtimeAssetsOnDisk();
				}
				else
				{
					MixtureGraphProcessor.RunOnce(mixture);
					mixture.SaveAllTextures(false);
				}

				ProjectWindowUtil.ShowCreatedAsset(mixture.mainOutputTexture);
				Selection.activeObject = mixture.mainOutputTexture;
				EditorApplication.delayCall += () => EditorGUIUtility.PingObject(mixture.mainOutputTexture);
			}
		}
	}
}
