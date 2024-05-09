using TerrainMixture.Runtime.Grid;
using UnityEngine;

namespace TerrainMixture.Runtime.Behaviours
{
	public class TerrainMixtureGridObserverBehaviour : MonoBehaviour
	{
		void Update()
		{
			ObserverPoint.Position = transform.position;
		}
	}
}
