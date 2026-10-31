namespace Exchange.Api.Services
{
    public interface IUtilityProcessLauncher
    {
        int Start(UtilityLaunchSnapshot snapshot);
    }
}