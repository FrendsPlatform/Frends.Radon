namespace Frends.Radon
{
    public interface IRadonConfigurationManager
    {
        IEmailConfiguration GetEmailConfig();
        IFilterConfiguration GetFilterConfig();
    }
}
