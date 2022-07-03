using System.Diagnostics;
using Autofac;
using CommandLine;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Generation.Tools.Common;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

/// <summary>
/// Confirms that there is no missing types included in FormLink interfaces.  It compares all formlinks and ensures that
/// if they point to an existing MajorRecord, that the typing defined in Mutagen allows them to connect.
/// </summary>
[Verb("formlink-inclusion-confirmation")]
public class FormLinkInclusionConfirmation : ISourceFileProvider, IGameReleaseContext, ITargetTypeStringProvider
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }
    
    [Option('e', "Exclusions", Required = false, HelpText = "File of major record formkeys to not analyze")]
    public string? ExclusionsPath { get; set; }
    
    [Option('t', "TargetRecordType", Required = false, HelpText = "Target record type to investigate. eg 'Weapon'")]
    public string? TargetRecordType { get; set; }

    public void Execute()
    {
        Stopwatch sw = new();
        sw.Start();
        ContainerBuilder builder = new ContainerBuilder();
        builder.RegisterModule<CommonModule>();
        builder.RegisterAssemblyTypes(typeof(FormLinkInclusionConfirmation).Assembly)
            .InNamespaceOf<FormLinkInclusionConfirmation>()
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterInstance(this).AsSelf().AsImplementedInterfaces();
        using var container = builder.Build();
        var executor = container.Resolve<Executor>();
        executor.Execute();
        Console.WriteLine($"Done {sw.ElapsedMilliseconds}ms");
    }
}