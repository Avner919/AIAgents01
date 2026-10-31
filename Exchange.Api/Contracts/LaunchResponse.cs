namespace Exchange.Api.Contracts
{
    public sealed record LaunchDefaults(string ClientFileName, string ClientExecutablePath,
        string ClientWorkingDirectory, string InputDirectory, string OutputDirectory, int TimeoutMilliseconds);

    public sealed record LaunchResponse(int ProcessId, LaunchDefaults Defaults);
}