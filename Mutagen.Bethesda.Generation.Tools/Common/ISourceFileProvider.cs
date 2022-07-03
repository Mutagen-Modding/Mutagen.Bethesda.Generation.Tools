using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Generation.Tools.Common;

public interface ISourceFileProvider
{
    ModPath SourceFile { get; }
}