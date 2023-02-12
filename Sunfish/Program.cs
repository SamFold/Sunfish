namespace Sunfish
{

    class Program
    {
        static void Main(string[] args)
        {
            var eNDCalculator = new ENDCalculator();
            var end = eNDCalculator.GetEquivalentNarcoticDepth(30, 30);

            var divePlanner = new DivePlanner(new ENDCalculator(), new PO2Calculator());

            divePlanner.RequestDiveOverview();
            var segments = divePlanner.ProcessDiveSegments();
            var cns = divePlanner.GetTotalCNSAccumulation();




            Console.WriteLine($"Program Ended here and pausing. Total CNS: {cns.ToString("F2")}");
            Console.ReadLine();
        }
    }

}

