namespace TerrainMixture.Runtime.Navigation
{
	public enum NavigationSupport
	{
		None,
#if UNITY_AI_NAVIGATION
		UnityAINavigation,
#endif
#if ANYPATH
		AnyPath,
#endif
	}
}
