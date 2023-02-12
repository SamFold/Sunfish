namespace Sunfish.Abstractions
{
    public interface IENDCalculator
    {
        decimal GetEquivalentNarcoticDepth(decimal heliumPercentage, decimal depth);
    }
}