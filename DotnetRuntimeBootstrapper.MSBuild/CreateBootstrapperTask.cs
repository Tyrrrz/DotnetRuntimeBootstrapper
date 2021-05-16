using Microsoft.Build.Utilities;

namespace DotnetRuntimeBootstrapper.MSBuild
{
    public class CreateBootstrapperTask : Task
    {
        public override bool Execute()
        {
            Log.LogMessage("Creating bootstrapper...");

            var properties = BuildEngine6.GetGlobalProperties();

            return true;
        }
    }
}