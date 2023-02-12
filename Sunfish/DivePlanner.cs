using System;
using Sunfish.Abstractions;

namespace Sunfish
{
    public class DivePlanner
    {

        public int TotalDiveSegments { get; private set; }
        private List<DiveSegment> _diveSegments = new List<DiveSegment>();

        private readonly IENDCalculator eNDCalculator;
        private readonly IPO2Calculator pO2Calculator;

        public DivePlanner(IENDCalculator eNDCalculator, IPO2Calculator pO2Calculator)
        {
            this.eNDCalculator = eNDCalculator;
            this.pO2Calculator = pO2Calculator;
        }

        public void RequestDiveOverview()
        {
            var message = "Input total number of dive segments";
            var totalSegments = GetIntegerInput(message);
            TotalDiveSegments = totalSegments;
        }

        public List<DiveSegment> ProcessDiveSegments()
        {
            var diveSegments = RequestDiveSegmentInformation();
            return diveSegments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public decimal GetTotalCNSAccumulation()
        {
            if(_diveSegments.Count == 0)
            {
                throw new InvalidDataException("No Dive Segment Data");
            }

            decimal totalCNS = 0m;

            foreach(var diveSegment in _diveSegments)
            {
                if (diveSegment.CNSAccumulated != null)
                {
                    totalCNS += diveSegment.CNSAccumulated.Value;
                }
                else
                {
                    throw new NullReferenceException("CNSAccumulated for Dive Segment is null");
                }
            }

            return totalCNS;
        }

        private List<DiveSegment> RequestDiveSegmentInformation()
        {
            for (int i = 0; i < TotalDiveSegments; i++)
            {
                var diveSegment = new DiveSegment();

                var durationMessage = $"Input time/duration of Dive Segment {i+1} (mins)";
                var duration = GetIntegerInput(durationMessage);

                diveSegment.Duration = duration;

                GetAdditionalDiveSegmentData(diveSegment);

                var CNSHit = CalculateCNSHitForSegment(diveSegment);

                diveSegment.CNSAccumulated = CNSHit;

                _diveSegments.Add(diveSegment);
            }

            return _diveSegments;
        }

        private decimal RequestPO2Data()
        {
            var message = "Please input the P02 value for the dive segment (e.g. 0.42)";
            var result = GetDecimalInput(message);
            return result;
        }

        public void GetAdditionalDiveSegmentData(DiveSegment diveSegment)
        {
            var requestType = RequestOxygenInformationFormat();

            if (requestType == 1) //Percentage and Depth
            {
                var percentageAndDepth = RequestPercentageAndDepthData();
                diveSegment.OxygenPercent = percentageAndDepth.Item1;
                diveSegment.Depth = percentageAndDepth.Item2;

                var P02 = CalculatePO2(percentageAndDepth.Item1, percentageAndDepth.Item2);
                diveSegment.PO2 = P02;
            }
            else if (requestType == 2) // P02 Directly
            {
                diveSegment.PO2 = RequestPO2Data();
            }
            else
            {
                throw new InvalidDataException("requestType for Oxygen Information should be 1 or 2");
            }
        }

        private (int, int) RequestPercentageAndDepthData()
        {
            var percentMessage = "Input the Percentage of Oxygen in the Gas Mix (e.g. 20)";
            var depthMessage = "Input the Water Depth in meters (e.g. 50)";

            var percent02 = GetIntegerInput(percentMessage);
            var depth = GetIntegerInput(depthMessage);

            return (percent02, depth);

        }

        private int RequestOxygenInformationFormat()
        {
            Console.WriteLine(@"Choose an option:
            1) Provide an Oxygen Percentage and Depth (e.g. 30% O2 at 40m)
            2) Provide a P02 value directly (e.g 0.42)
            Type '1' or '2' and press enter.");

            int parsedInput;

            while (true)
            {
                var input = Console.ReadLine();

                var inputParsed = int.TryParse(input, out parsedInput);

                if (input == null || !inputParsed)
                {
                    Console.WriteLine("Input empty or parsed unsuccessfully. Please choose a valid option.");
                }
                else if (parsedInput != 1 && parsedInput != 2)
                {
                    Console.WriteLine("Value must be 1 or 2. Please choose a valid option.");
                }
                else
                {
                    break;
                }
            }

            return parsedInput;
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

            if(duration == null)
            {
                throw new ArgumentNullException("Duration value cannot be null when calculating CNS hit");
            }

            if (PO2 == null)
            {
                throw new ArgumentNullException("PO2 value cannot be null when calculating CNS hit");
            }

            if(PO2 > 1.6m)
            {
                throw new ArgumentOutOfRangeException("PO2 value cannot be greater than 1.6. (Safety)");
            }

            if(CNSHelper.NOAACNSData.ContainsKey(PO2.Value))
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
                    if(found)
                    {
                        calculatedNextPO2 = temp;
                        totalAllowedTime = CNSHelper.NOAACNSData[calculatedNextPO2];
                        break;
                    }

                    if(temp == PO2.Value)
                    {
                        found = true;
                    }
                }
            }

            if(totalAllowedTime != -1)
            {
                percentCNSAccumulated = ((decimal) duration.Value / (decimal) totalAllowedTime) * 100m;
            }
            else
            {
                throw new Exception("Couldn't calculate Max CNS Value from NOAA Table");
            }

            return percentCNSAccumulated;
        }

        /// <summary>
        /// Generic method to retrieve some integer input from the user
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public int GetIntegerInput(string message)
        {
            Console.WriteLine(message);

            var input = Console.ReadLine();

            int parsedInput;
            var inputParsed = int.TryParse(input, out parsedInput);

            if (input == null || !inputParsed)
            {
                Console.WriteLine("Input empty or parsed unsuccessfully");
                throw new InvalidDataException();
            }
            else if (parsedInput < 0)
            {
                Console.WriteLine("Negative input values are not valid");
                throw new InvalidDataException();
            }

            return parsedInput;
        }

        /// <summary>
        /// Generic Method to retrieve some decimal input from the user
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public decimal GetDecimalInput(string message)
        {
            Console.WriteLine(message);

            var input = Console.ReadLine();

            decimal parsedInput;
            var inputParsed = decimal.TryParse(input, out parsedInput);

            if (input == null || !inputParsed)
            {
                Console.WriteLine("Input empty or parsed unsuccessfully");
                throw new InvalidDataException();
            }
            else if (parsedInput < 0)
            {
                Console.WriteLine("Negative input values are not valid");
                throw new InvalidDataException();
            }

            return parsedInput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentO2"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public decimal CalculatePO2(int percentO2, int depth)
        {
            decimal O2 = (decimal) percentO2 / 100m;
            decimal atmospheres = ((decimal) depth / 10m) + 1m;
            var result = O2 * atmospheres;
            return result;
        }
    }
}

