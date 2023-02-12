using System;
using Sunfish.Abstractions;


namespace Sunfish
{
	public class PO2Calculator : IPO2Calculator
	{
		public PO2Calculator()
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentO2"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public decimal CalculatePO2(int percentO2, int depth)
        {
            decimal O2 = (decimal)percentO2 / 100m;
            decimal atmospheres = ((decimal)depth / 10m) + 1m;
            var result = O2 * atmospheres;
            return result;
        }

        /// <summary>
        /// Calculte the CNS hit (in percent) for a given Dive Segment (from NOAA tables)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public decimal CalculateCNSHitForSegment(DiveSegment segment)
        {
            var PO2 = segment.PO2;
            int totalAllowedTime = -1;
            var duration = segment.Duration;
            decimal percentCNSAccumulated;

            if (duration == null)
            {
                throw new ArgumentNullException("Duration value cannot be null when calculating CNS hit");
            }

            if (PO2 == null)
            {
                throw new ArgumentNullException("PO2 value cannot be null when calculating CNS hit");
            }

            if (PO2 > 1.6m)
            {
                throw new ArgumentOutOfRangeException("PO2 value cannot be greater than 1.6. (Safety)");
            }

            if (CNSHelper.NOAACNSData.ContainsKey(PO2.Value))
            {
                totalAllowedTime = CNSHelper.NOAACNSData[PO2.Value];
            }
            else
            {
                var PO2s = CNSHelper.NOAACNSData.Keys.ToList();
                PO2s.Add(PO2.Value);
                PO2s.Sort();

                var found = false;
                decimal calculatedNextPO2;

                foreach (var temp in PO2s)
                {
                    if (found)
                    {
                        calculatedNextPO2 = temp;
                        totalAllowedTime = CNSHelper.NOAACNSData[calculatedNextPO2];
                        break;
                    }

                    if (temp == PO2.Value)
                    {
                        found = true;
                    }
                }
            }

            if (totalAllowedTime != -1)
            {
                percentCNSAccumulated = ((decimal)duration.Value / (decimal)totalAllowedTime) * 100m;
            }
            else
            {
                throw new Exception("Couldn't calculate Max CNS Value from NOAA Table");
            }

            return percentCNSAccumulated;
        }


    }
}

