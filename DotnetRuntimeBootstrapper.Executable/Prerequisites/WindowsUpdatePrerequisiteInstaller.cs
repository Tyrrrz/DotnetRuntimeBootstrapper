﻿using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public class WindowsUpdatePrerequisiteInstaller : IPrerequisiteInstaller
    {
        public IPrerequisite Prerequisite { get; }

        public string FilePath { get; }

        public WindowsUpdatePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
        {
            Prerequisite = prerequisite;
            FilePath = filePath;
        }

        public void Run()
        {
            CommandLine.Run("wusa", new[] {FilePath, "/quiet", "/norestart"}, true);

            // Regardless of the outcome, record the fact that this update has been installed,
            // so we don't attempt to install it again in the future.
            // We need to do this because an update may fail to install if it was superseded
            // by some other already installed update which we are not aware of.
            // https://github.com/Tyrrrz/LightBulb/issues/209
            InstallationHistory.Record(Prerequisite.Id);
        }
    }
}