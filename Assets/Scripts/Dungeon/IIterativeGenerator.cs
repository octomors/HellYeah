public interface IIterativeGenerator
{
    /// <summary>
    /// returns the next point in the generation sequence
    /// </summary>
    /// <returns></returns>
    public (int y, int x) GetNextPoint();
}