namespace Frends.Radon
{
    public interface IFilterItem
    {
        bool Matches(LogEvent logEvent);
    }
}