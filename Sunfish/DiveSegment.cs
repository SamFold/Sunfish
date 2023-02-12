using System;
namespace Sunfish
{
	public class DiveSegment
	{
		public int? Depth { get; set; }
		public int? OxygenPercent { get; set; }
		public decimal? PO2 { get; set; }
		public int? Duration { get; set; }
		public decimal? CNSAccumulated { get; set; }

		public DiveSegment()
		{
		}

		public DiveSegment(int depth, int oxygenPercent, int duration)
		{
			Depth = depth;
			OxygenPercent = oxygenPercent;
			Duration = duration;
		}

		public DiveSegment(decimal pO2, int duration)
		{
			PO2 = pO2;
			Duration = duration;
		}

	}
}

