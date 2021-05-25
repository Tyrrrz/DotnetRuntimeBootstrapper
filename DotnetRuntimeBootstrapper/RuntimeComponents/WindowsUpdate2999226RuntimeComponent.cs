using System.Diagnostics;
using DotnetRuntimeBootstrapper.Env;
using OperatingSystem = DotnetRuntimeBootstrapper.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.RuntimeComponents
{
    public class WindowsUpdate2999226RuntimeComponent : IRuntimeComponent
    {
        public string DisplayName => "Windows Update KB2999226";

        public bool IsRebootRequired => false;

        public bool CheckIfInstalled() =>
            OperatingSystem.Version >= OperatingSystemVersion.Windows10 ||
            OperatingSystem.IsUpdateInstalled("KB2999226");

        public string GetInstallerDownloadUrl()
        {
            switch (OperatingSystem.ProcessorArchitecture)
            {
                case ProcessorArchitecture.X86:
                case ProcessorArchitecture.Arm:
                {
                    switch (OperatingSystem.Version)
                    {
                        case OperatingSystemVersion.Windows7:
                            return
                                "http://download.microsoft.com/download/4/F/E/4FE73868-5EDD-4B47-8B33-CE1BB7B2B16A/Windows6.1-KB2999226-x86.msu";
                        case OperatingSystemVersion.Windows8:
                            return
                                "http://download.microsoft.com/download/1/E/8/1E8AFE90-5217-464D-9292-7D0B95A56CE4/Windows8-RT-KB2999226-x86.msu";
                        default:
                            return
                                "http://download.microsoft.com/download/E/4/6/E4694323-8290-4A08-82DB-81F2EB9452C2/Windows8.1-KB2999226-x86.msu";
                    }
                }
                default:
                {
                    switch (OperatingSystem.Version)
                    {
                        case OperatingSystemVersion.Windows7:
                            return
                                "http://download.microsoft.com/download/1/1/5/11565A9A-EA09-4F0A-A57E-520D5D138140/Windows6.1-KB2999226-x64.msu";
                        case OperatingSystemVersion.Windows8:
                            return
                                "http://download.microsoft.com/download/A/C/1/AC15393F-A6E6-469B-B222-C44B3BB6ECCC/Windows8-RT-KB2999226-x64.msu";
                        default:
                            return
                                "http://download.microsoft.com/download/9/6/F/96FD0525-3DDF-423D-8845-5F92F4A6883E/Windows8.1-KB2999226-x64.msu";
                    }
                }
            }
        }

        public void RunInstaller(string installerFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "wusa",
                Arguments = installerFilePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            using (var process = new Process{StartInfo = startInfo})
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }
}