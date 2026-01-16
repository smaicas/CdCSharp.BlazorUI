using CdCSharp.DocGen.Core.Models.Generation;

namespace CdCSharp.DocGen.Core.Abstractions.Formatting;

public interface ILlmDocComposer
{
    Task<string> ComposeAsync(GenerationContext context);
}