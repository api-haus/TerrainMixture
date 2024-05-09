using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.Nodes
{
	[Documentation(@"
Set Splat Output.
")]
	[System.Serializable]
	[NodeMenuItem("Terrain/Splat Output")]
	public class TerrainSplatOutputNode : MixtureNode
	{
		public override string name => "Terrain Splat (Alphamap) Output";
		public override bool showDefaultInspector => true;

		[Input] public RenderTexture splatOutput;
		[HideInInspector] public int splatIndex;
		[ShowInInspector] public TerrainLayer[] terrainLayers = new TerrainLayer[4];

		public override bool canProcess => splatOutput != null && terrainLayers is { Length: 4 } &&
		                                   terrainLayers.All(x => x != null);

		// TODO: move this to NodeGraphProcessor
		[NonSerialized] protected HashSet<string> uniqueMessages = new HashSet<string>();

		protected override void Enable()
		{
			base.Enable();
			UpdateMessages();
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd)) return false;
			UpdateMessages();
			return true;
		}

		void UpdateMessages()
		{
			if (!canProcess)
			{
				if (uniqueMessages.Add("OutputNotConnected"))
					AddMessage("Select this node to reveal Inspector properties. Attach all 4 Terrain Layers.",
						NodeMessageType.Warning);
			}
			else
			{
				uniqueMessages.Clear();
				ClearMessages();
			}
		}
	}
}
