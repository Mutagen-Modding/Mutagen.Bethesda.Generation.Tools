using Autofac;
using Mutagen.Bethesda.Generation.Tools.Common;

namespace Mutagen.Bethesda.Generation.Tools;

public class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(EnvironmentProvider).Assembly)
            .InNamespaceOf<EnvironmentProvider>()
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}