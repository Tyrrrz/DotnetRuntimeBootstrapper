namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public interface IRuntimeComponent
    {
        string DisplayName { get; }

        bool IsRebootRequired { get; }

        bool CheckIfInstalled();

        string GetInstallerDownloadUrl();

        void RunInstaller(string installerFilePath);
    }
}