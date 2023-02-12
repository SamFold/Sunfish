using System;
using Sunfish.Abstractions;

namespace Sunfish
{
    public class ENDCalculator : IENDCalculator
	{
		public ENDCalculator()
		{
		}

		public decimal GetEquivalentNarcoticDepth(decimal heliumPercentage, decimal depth)
		{
            decimal atmospheres = ((decimal)depth / 10m) + 1m;

			var heliumDecimal = heliumPercentage / 100m;

			var end = (1m - heliumDecimal) * atmospheres;

			var endinMeters = (end - 1) * 10;

			return endinMeters;
        }
    }
}

