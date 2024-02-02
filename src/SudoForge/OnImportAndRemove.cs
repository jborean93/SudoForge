using System.Management.Automation;
using RemoteForge;
using SudoForge;

namespace TestForge;

public class OnModuleImportAndRemove : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    public void OnImport()
    {
        RemoteForgeRegistration.Register(typeof(SudoInfo).Assembly);
    }

    public void OnRemove(PSModuleInfo module)
    {
        RemoteForgeRegistration.Unregister(SudoInfo.ForgeName);
    }
}
