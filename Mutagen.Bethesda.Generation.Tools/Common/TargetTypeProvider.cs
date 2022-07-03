using Loqui;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.Common;

public interface ITargetTypeStringProvider
{
    string? TargetRecordType { get; }
}

public class TargetTypeProvider
{
    private readonly IGameReleaseContext _gameReleaseContext;
    private readonly ITargetTypeStringProvider _typeStringProvider;

    public TargetTypeProvider(
        IGameReleaseContext gameReleaseContext,
        ITargetTypeStringProvider typeStringProvider)
    {
        _gameReleaseContext = gameReleaseContext;
        _typeStringProvider = typeStringProvider;
    }
    
    public Type GetTargetType()
    {
        if (_typeStringProvider.TargetRecordType.IsNullOrWhitespace())
        {
            return typeof(IMajorRecordGetter);
        }

        var regis = LoquiRegistration.GetRegisterByFullName(
            $"Mutagen.Bethesda.{_gameReleaseContext.Release}.{_typeStringProvider.TargetRecordType}");
        if (regis == null)
        {
            throw new ArgumentException($"Unknown record name: {_typeStringProvider.TargetRecordType}");
        }

        return regis.GetterType;
    }
}