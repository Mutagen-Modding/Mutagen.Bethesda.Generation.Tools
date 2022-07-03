using FluentAssertions;
using Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Masters;
using Mutagen.Bethesda.Plugins.Meta;
using Xunit;

namespace Mutagen.Bethesda.Generation.Tools.Tests.InclusionConfirmation;

public class LocationPrinterTests
{
    [Fact]
    public void SimpleFormKeyMember()
    {
        Warmup.Init();
        var data = File.ReadAllBytes("InclusionConfirmation/Files/SimpleFormKeyMember");
        var masters = new MasterReferenceCollection("Fallout4.esm");
        var book = Book.CreateFromBinary(new MutagenFrame(
            new MutagenMemoryReadStream(data, new ParsingBundle(GameConstants.Fallout4, masters))));
        var ret = new Stack<string>();
        new LocationRetriever().RetrieveLocationOf(book, new FormKey("Fallout4.esm", 0x2488FF), ret);
        ret.Should().Equal("PreviewTransform");
    }
    
    [Fact]
    public void FormKeyList()
    {
        Warmup.Init();
        var data = File.ReadAllBytes("InclusionConfirmation/Files/FormKeyList");
        var masters = new MasterReferenceCollection("Fallout4.esm");
        var book = LeveledItem.CreateFromBinary(new MutagenFrame(
            new MutagenMemoryReadStream(data, new ParsingBundle(GameConstants.Fallout4, masters))));
        var ret = new Stack<string>();
        new LocationRetriever().RetrieveLocationOf(book, new FormKey("Fallout4.esm", 0x18102D), ret);
        ret.Should().Equal("Reference", "Data", "Entries[2]");
    }
}