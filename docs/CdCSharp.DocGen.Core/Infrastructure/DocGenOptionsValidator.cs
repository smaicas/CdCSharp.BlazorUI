// Infrastructure/DocGenOptionsValidator.cs
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Options;

namespace CdCSharp.DocGen.Core.Infrastructure;

public class DocGenOptionsValidator : IValidateOptions<DocGenOptions>
{
    public ValidateOptionsResult Validate(string? name, DocGenOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(options.ProjectPath))
        {
            failures.Add("ProjectPath is required");
        }
        else if (!Directory.Exists(options.ProjectPath))
        {
            failures.Add($"ProjectPath does not exist: {options.ProjectPath}");
        }

        if (options.Ai.Provider == AiProviderType.Groq &&
            string.IsNullOrWhiteSpace(options.Ai.ApiKey))
        {
            failures.Add("Groq API key is required when using Groq provider");
        }

        if (options.Ai.Provider == AiProviderType.LMStudio &&
            string.IsNullOrWhiteSpace(options.Ai.BaseUrl))
        {
            failures.Add("LMStudio BaseUrl is required when using LMStudio provider");
        }

        if (options.Conversation.SlidingWindowSize < 2)
        {
            failures.Add("SlidingWindowSize must be at least 2");
        }

        if (options.Conversation.CompressionThreshold < 1)
        {
            failures.Add("CompressionThreshold must be at least 1");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}