using CdCSharp.DocGen.Core.Models.Generation;

namespace CdCSharp.DocGen.Core.Abstractions.Formatting;

public interface IHumanDocComposer
{
    string Compose(GenerationContext context);
}