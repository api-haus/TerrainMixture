using System;

namespace TerrainMixture.Tasks.Pacing
{
	public class PacingScope : IDisposable
	{
		public readonly int WasBefore;

		public PacingScope(Pacing pacing)
		{
			WasBefore = PacingController.CurrentMaxFrameSkip;
			PacingController.CurrentMaxFrameSkip = PacingController.GetPacingTime(pacing);
		}

		public PacingScope(int newValue)
		{
			WasBefore = PacingController.CurrentMaxFrameSkip;
			PacingController.CurrentMaxFrameSkip = newValue;
		}

		public void Dispose()
		{
			PacingController.CurrentMaxFrameSkip = WasBefore;
		}
	}
}
