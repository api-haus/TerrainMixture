namespace TerrainMixture.Tasks.Pacing
{
	public static class PacingController
	{
		/// <summary>
		/// In realtime we prioritise realtime.
		/// </summary>
		const int DefaultFrameSkipRealtime = 1;

		/// <summary>
		/// Just enough so Windows doesn't think that app is lagging.
		/// </summary>
		const int DefaultFrameSkipNowLoading = 9999;

		public static int CurrentMaxFrameSkip = 1;

		public static void EnterRealtime(int maxFrameSkip = DefaultFrameSkipRealtime)
		{
			CurrentMaxFrameSkip = maxFrameSkip;
		}

		public static void EnterLoading(int maxFrameSkip = DefaultFrameSkipNowLoading)
		{
			CurrentMaxFrameSkip = maxFrameSkip;
		}

		public static int GetPacingTime(Pacing pacing)
		{
			return pacing switch
			{
				Pacing.Realtime => DefaultFrameSkipRealtime,
				Pacing.DuringLoading => DefaultFrameSkipNowLoading,
				_ => DefaultFrameSkipRealtime
			};
		}
	}

	public enum Pacing
	{
		Realtime,
		DuringLoading,
	}
}
