using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Agents;

public partial class AgentExecutor
{
    private readonly LMStudioClient _aiClient;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;
    private readonly CompressionAgent _compressionAgent;
    private readonly GeneratedFilesTracker _filesTracker;

    private const int MaxFormatRetries = 2;

    public AgentExecutor(
        LMStudioClient aiClient,
        TheonLogger logger,
        TheonOptions options,
        CompressionAgent compressionAgent,
        GeneratedFilesTracker filesTracker)
    {
        _aiClient = aiClient;
        _logger = logger;
        _options = options;
        _compressionAgent = compressionAgent;
        _filesTracker = filesTracker;
    }

    public async Task<AgentExecutionResult> ExecuteAsync(Agent agent, string instruction, string? agentsSummary = null)
    {
        agent.LastActiveAt = DateTime.UtcNow;

        string filesContext = _filesTracker.GetFilesContext(agent.Id);

        string fullInstruction = BuildFullInstruction(instruction, agentsSummary, filesContext, agent.Id);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = fullInstruction
        });

        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Input, fullInstruction);

        await CompressIfNeededAsync(agent);

        string traceFile = _logger.TracePrompt(agent.Id, fullInstruction);

        string response = await _aiClient.SendAsync(agent.ConversationHistory.ToList());

        _logger.UpdateTrace(traceFile, response);
        _logger.LogAgentInteraction(agent.Id, InteractionDirection.Output, response);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = response
        });

        AgentExecutionResult result = ParseResponse(agent.Id, response);

        // Validar formato y reintentar si hay errores
        FormatValidationResult validation = ValidateResponseFormat(response, result);

        if (!validation.IsValid)
        {
            _logger.Warning($"Format validation failed for agent {agent.Id}: {string.Join(", ", validation.Errors)}");

            for (int retry = 0; retry < MaxFormatRetries && !validation.IsValid; retry++)
            {
                _logger.Info($"Requesting format correction (attempt {retry + 1}/{MaxFormatRetries})");

                string correctionPrompt = BuildFormatCorrectionPrompt(validation.Errors);

                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.User,
                    Content = correctionPrompt
                });

                response = await _aiClient.SendAsync(agent.ConversationHistory.ToList());

                _logger.LogAgentInteraction(agent.Id, InteractionDirection.Output, $"[RETRY {retry + 1}] {response}");

                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.Assistant,
                    Content = response
                });

                result = ParseResponse(agent.Id, response);
                validation = ValidateResponseFormat(response, result);
            }

            if (!validation.IsValid)
            {
                _logger.Error($"Agent {agent.Id} failed to correct format after {MaxFormatRetries} retries");
                result.FormatErrors = validation.Errors;
            }
        }

        LogParsedFiles(agent.Id, result);

        if (result.GeneratedFiles.Count > 0)
        {
            _filesTracker.RecordFiles(agent.Id, result.GeneratedFiles);
        }

        return result;
    }

    private string BuildFullInstruction(string instruction, string? agentsSummary, string filesContext, string currentAgentId)
    {
        if (!string.IsNullOrEmpty(agentsSummary))
        {
            // ✅ MEJORADO: Filtrar el agente actual de la lista para evitar auto-referencia
            string filteredAgentsSummary = FilterCurrentAgent(agentsSummary, currentAgentId);

            return $"""
            # Available Agents

            {filteredAgentsSummary}

            ---

            {filesContext}

            ---

            # Your Task

            {instruction}
            """;
        }

        return $"""
        {filesContext}

        ---

        {instruction}
        """;
    }

    private string BuildFormatCorrectionPrompt(List<string> errors)
    {
        return $"""
FORMAT ERROR

Your response could not be parsed. Fix the following errors:

{string.Join("\n", errors.Select(e => $"- {e}"))}

CORRECT SYNTAX EXAMPLES

Requesting file list:
[REQUEST_FILE_PATHS: assembly=""]

Requesting file content:
[REQUEST_FILE: path="Services/MyService.cs"]

Querying another agent (use exact ID from Available Agents list):
[QUERY_AGENT: id="a1b2c3d4" question="What should I do?"]

Creating a new agent:
[CREATE_AGENT: name="Database Expert" expertise="database design" files=""]

Generating a file:
[GENERATE_FILE: name="README.md" language="markdown"]
content here
[/GENERATE_FILE]

When task is complete:
[TASK_COMPLETE]
[CONFIDENCE: 0.85]

IMPORTANT:
- For QUERY_AGENT, use the exact 8-character ID from "Available Agents"
- Do NOT use expertise names as IDs
- Do NOT query yourself

Provide your corrected response now.
""";
    }

    private string FilterCurrentAgent(string agentsSummary, string currentAgentId)
    {
        if (string.IsNullOrEmpty(agentsSummary))
            return "No other agents available.";

        // Filtrar líneas que contengan el ID del agente actual
        string[] lines = agentsSummary.Split('\n');
        List<string> filteredLines = [];

        foreach (string line in lines)
        {
            // Mantener headers y líneas que NO contengan el ID actual
            if (!line.Contains(currentAgentId))
            {
                filteredLines.Add(line);
            }
        }

        string filtered = string.Join("\n", filteredLines).Trim();

        // Si no quedan agentes después de filtrar
        if (filtered == "Current agents:" || string.IsNullOrWhiteSpace(filtered))
        {
            return "No other agents available. Use CREATE_AGENT if you need specialized help.";
        }

        return filtered;
    }

    private FormatValidationResult ValidateResponseFormat(string response, AgentExecutionResult result)
    {
        List<string> errors = [];

        // Detectar GENERATE_FILE sin cierre
        MatchCollection openGenerateFile = Regex.Matches(response, @"\[GENERATE_FILE:\s*name=""([^""]+)""", RegexOptions.IgnoreCase);
        int closeGenerateFile = Regex.Matches(response, @"\[/GENERATE_FILE\]", RegexOptions.IgnoreCase).Count;

        if (openGenerateFile.Count > closeGenerateFile)
        {
            int unclosed = openGenerateFile.Count - closeGenerateFile;
            List<string> unclosedFiles = openGenerateFile
                .Cast<Match>()
                .Skip(closeGenerateFile)
                .Select(m => m.Groups[1].Value)
                .ToList();

            errors.Add($"Unclosed [GENERATE_FILE] tag(s): {string.Join(", ", unclosedFiles)}. Must include [/GENERATE_FILE]");
        }

        // Detectar APPEND_TO_FILE sin cierre
        MatchCollection openAppend = Regex.Matches(response, @"\[APPEND_TO_FILE:\s*name=""([^""]+)""", RegexOptions.IgnoreCase);
        int closeAppend = Regex.Matches(response, @"\[/APPEND_TO_FILE\]", RegexOptions.IgnoreCase).Count;

        if (openAppend.Count > closeAppend)
        {
            int unclosed = openAppend.Count - closeAppend;
            errors.Add($"Unclosed [APPEND_TO_FILE] tag(s). Must include [/APPEND_TO_FILE]");
        }

        // Detectar tags anidados
        foreach (Match match in GeneratedFileRegex().Matches(response))
        {
            string content = match.Groups[3].Value;
            if (Regex.IsMatch(content, @"\[GENERATE_FILE:", RegexOptions.IgnoreCase))
            {
                string fileName = match.Groups[1].Value;
                errors.Add($"Nested [GENERATE_FILE] inside '{fileName}'. Nesting is not allowed.");
            }
        }

        // ✅ MEJORADO: Validación de sintaxis malformada más específica
        // Detectar texto precediendo tags (como **REQUEST_FILE en lugar de [REQUEST_FILE])
        string[] malformedPatterns = [
            @"(?<!\[)\*\*\s*REQUEST_FILE",           // **REQUEST_FILE sin [
        @"(?<!\[)\*\*\s*GENERATE_FILE",          // **GENERATE_FILE sin [
        @"(?<!\[)\*\*\s*QUERY_AGENT",            // **QUERY_AGENT sin [
        @"(?<!\[)\*\*\s*CREATE_AGENT",           // **CREATE_AGENT sin [
        @"REQUEST_FILE_PATHS[^:\[\]]",           // REQUEST_FILE_PATHS sin : o []
        @"REQUEST_FILE(?!\s*:|\s*\])[^:\[\]]",   // REQUEST_FILE sin : o ]
    ];

        foreach (string pattern in malformedPatterns)
        {
            if (Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase))
            {
                errors.Add("Malformed tool syntax detected. Use exact format: [TOOL_NAME: param=\"value\"]");
                break; // Solo reportar una vez
            }
        }

        // CONFIDENCE solo es requerido si:
        // 1. No hay solicitudes pendientes (el agente no está pidiendo información)
        // 2. O el agente marcó TASK_COMPLETE
        bool hasTaskComplete = TaskCompleteRegex().IsMatch(response);
        bool hasConfidence = ConfidenceRegex().IsMatch(response);
        bool hasPendingRequests = result.HasPendingRequests;

        if (!hasPendingRequests && !hasConfidence)
        {
            errors.Add("Response appears complete but missing [CONFIDENCE: X.X]. Add confidence level at the end.");
        }

        if (hasTaskComplete && !hasConfidence)
        {
            errors.Add("[TASK_COMPLETE] used but missing [CONFIDENCE: X.X]. Both are required when completing a task.");
        }

        // Verificar que los archivos se parsearon correctamente
        if (openGenerateFile.Count > 0 && result.GeneratedFiles.Count == 0 && closeGenerateFile > 0)
        {
            errors.Add("File generation tags found but no files were parsed. Check tag format.");
        }

        return new FormatValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
    private void LogParsedFiles(string agentId, AgentExecutionResult result)
    {
        _logger.Debug($"=== Parse Results for Agent {agentId} ===");
        _logger.Debug($"  File requests: {result.FileRequests.Count}");
        _logger.Debug($"  File path requests: {result.FilePathsRequests.Count}");
        _logger.Debug($"  Agent queries: {result.AgentQueries.Count}");
        _logger.Debug($"  Agent creation requests: {result.AgentCreationRequests.Count}");
        _logger.Debug($"  Generated files: {result.GeneratedFiles.Count}");

        if (result.GeneratedFiles.Count > 0)
        {
            foreach (GeneratedFile file in result.GeneratedFiles)
            {
                _logger.Debug($"    ✓ Parsed: {file.FileName} ({file.Language}) - {file.Content.Length} chars");
            }
        }

        if (result.FormatErrors.Count > 0)
        {
            _logger.Warning($"  Format errors: {result.FormatErrors.Count}");
            foreach (string error in result.FormatErrors)
            {
                _logger.Warning($"    ✗ {error}");
            }
        }

        _logger.Debug($"  Confidence: {result.Confidence:P0}");
        _logger.Debug($"  Has pending requests: {result.HasPendingRequests}");
    }

    private async Task CompressIfNeededAsync(Agent agent)
    {
        ConversationOptions opts = _options.Conversation;

        int messageCount = agent.ConversationHistory.Count(m => m.Role != MessageRole.System);
        if (messageCount < opts.CompressionThreshold)
            return;

        _logger.Debug($"Compressing conversation for {agent.Name} ({messageCount} messages)");

        ConversationMessage? systemMsg = agent.ConversationHistory
            .FirstOrDefault(m => m.Role == MessageRole.System);

        List<ConversationMessage> nonSystemMessages = agent.ConversationHistory
            .Where(m => m.Role != MessageRole.System)
            .ToList();

        int messagesToCompress = nonSystemMessages.Count - opts.MessagesToKeep;
        if (messagesToCompress <= 0)
            return;

        List<ConversationMessage> toCompress = nonSystemMessages
            .Take(messagesToCompress)
            .ToList();

        List<ConversationMessage> toKeep = nonSystemMessages
            .TakeLast(opts.MessagesToKeep)
            .ToList();

        string summary = await _compressionAgent.CompressAsync(toCompress);

        agent.ConversationHistory.Clear();

        if (systemMsg != null)
            agent.ConversationHistory.Add(systemMsg);

        string filesContext = _filesTracker.GetFilesContext(agent.Id);

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"""
                # Previous Conversation Summary

                {summary}

                ---

                {filesContext}

                ---

                Continue from this context.
                """
        });

        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = "I understand the context from our previous conversation and the files I've generated. Ready to continue."
        });

        agent.ConversationHistory.AddRange(toKeep);

        _logger.Debug($"Compressed {messagesToCompress} messages, kept {opts.MessagesToKeep}. Total: {agent.ConversationHistory.Count}");
    }

    private AgentExecutionResult ParseResponse(string agentId, string response)
    {
        AgentExecutionResult result = new() { AgentId = agentId, RawResponse = response };

        if (IsCorruptedResponse(response))
        {
            _logger.Warning($"Corrupted response detected from agent {agentId}");
            result.Confidence = 0.0f;
            result.CleanContent = "[Error: Invalid response from model. Please try again.]";
            return result;
        }

        // Parse file requests
        foreach (Match match in FileRequestRegex().Matches(response))
        {
            result.FileRequests.Add(match.Groups[1].Value);
        }

        foreach (Match match in FilePathsRequestRegex().Matches(response))
        {
            result.FilePathsRequests.Add(match.Groups[1].Value);
        }

        // ✅ ACTUALIZADO: Parse agent queries por ID
        foreach (Match match in QueryAgentRegex().Matches(response))
        {
            result.AgentQueries.Add(new AgentRequest
            {
                Type = AgentRequestType.QueryAgent,
                FromAgentId = agentId,
                TargetAgentId = match.Groups[1].Value,  // ✅ ID en lugar de expertise
                TargetExpertise = null,                  // ✅ Ya no se usa
                Payload = match.Groups[2].Value,
                Reason = "Cross-agent query by ID"
            });
        }

        // Parse agent creation requests
        foreach (Match match in CreateAgentRegex().Matches(response))
        {
            string filesStr = match.Groups.Count > 3 ? match.Groups[3].Value : "";
            result.AgentCreationRequests.Add(new AgentCreationSpec
            {
                Name = match.Groups[1].Value,
                Expertise = match.Groups[2].Value,
                InitialContextFiles = string.IsNullOrEmpty(filesStr)
                    ? []
                    : filesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                ParentAgentId = agentId
            });
        }

        // Parse generated files
        foreach (Match match in GeneratedFileRegex().Matches(response))
        {
            string fileName = match.Groups[1].Value;
            string language = match.Groups[2].Value;
            string content = match.Groups[3].Value.Trim();

            result.GeneratedFiles.Add(new GeneratedFile
            {
                FileName = fileName,
                Language = language,
                Content = content
            });

            _logger.Debug($"Parsed GENERATE_FILE: {fileName} ({language}) - {content.Length} chars");
        }

        // Parse append to file
        foreach (Match match in AppendToFileRegex().Matches(response))
        {
            string fileName = match.Groups[1].Value;
            string contentToAppend = match.Groups[2].Value.Trim();

            GeneratedFile? existing = result.GeneratedFiles.FirstOrDefault(f => f.FileName == fileName);

            if (existing != null)
            {
                result.GeneratedFiles.Remove(existing);
                result.GeneratedFiles.Add(new GeneratedFile
                {
                    FileName = fileName,
                    Language = existing.Language,
                    Content = existing.Content + "\n\n" + contentToAppend
                });
            }
            else
            {
                string? trackedContent = _filesTracker.GetFileContent(agentId, fileName);

                if (trackedContent != null)
                {
                    result.GeneratedFiles.Add(new GeneratedFile
                    {
                        FileName = fileName,
                        Language = Path.GetExtension(fileName).TrimStart('.'),
                        Content = trackedContent + "\n\n" + contentToAppend
                    });
                }
                else
                {
                    result.GeneratedFiles.Add(new GeneratedFile
                    {
                        FileName = fileName,
                        Language = Path.GetExtension(fileName).TrimStart('.'),
                        Content = contentToAppend
                    });
                }
            }

            _logger.Debug($"Parsed APPEND_TO_FILE: {fileName} - {contentToAppend.Length} chars appended");
        }

        // Parse validation suggestions
        Match validationMatch = ValidationRegex().Match(response);
        if (validationMatch.Success)
        {
            result.SuggestedValidators = validationMatch.Groups[1].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        // Parse confidence
        Match confidenceMatch = ConfidenceRegex().Match(response);
        if (confidenceMatch.Success && float.TryParse(confidenceMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float conf))
        {
            result.Confidence = Math.Clamp(conf, 0f, 1f);
        }
        else
        {
            if (result.HasPendingRequests)
            {
                result.Confidence = -1f;
                _logger.Debug("No confidence in response - agent has pending requests");
            }
            else
            {
                result.Confidence = 0.1f;
                _logger.Warning("No confidence found in completed response, defaulting to 0.1");
            }
        }

        result.CleanContent = CleanResponse(response);

        return result;
    }
    private static string CleanFileContent(string content)
    {
        // Eliminar bloques markdown que el LLM pueda haber añadido incorrectamente
        content = Regex.Replace(content, @"^```\w*\s*\n", "", RegexOptions.Multiline);
        content = Regex.Replace(content, @"\n```\s*$", "", RegexOptions.Multiline);
        return content.Trim();
    }

    private static bool IsCorruptedResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return true;

        string[] corruptPatterns =
        [
            "<|channel|>",
            "<|im_start|>",
            "<|im_end|>",
            "<|endoftext|>",
            "<|assistant|>",
            "<|user|>",
            "<|system|>"
        ];

        return corruptPatterns.Any(p => response.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static string CleanResponse(string response)
    {
        string clean = response;

        clean = ConfidenceRegex().Replace(clean, "");
        clean = FileRequestRegex().Replace(clean, "");
        clean = QueryAgentRegex().Replace(clean, "");
        clean = CreateAgentRegex().Replace(clean, "");
        clean = ValidationRegex().Replace(clean, "");
        clean = FilePathsRequestRegex().Replace(clean, "");
        clean = TaskCompleteRegex().Replace(clean, "");

        clean = Regex.Replace(clean, @"\[GENERATE_FILE:.*?\][\s\S]*?\[/GENERATE_FILE\]", "", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"\[APPEND_TO_FILE:.*?\][\s\S]*?\[/APPEND_TO_FILE\]", "", RegexOptions.IgnoreCase);

        while (clean.Contains("\n\n\n"))
            clean = clean.Replace("\n\n\n", "\n\n");

        return clean.Trim();
    }

    [GeneratedRegex(@"\[QUERY_AGENT:\s*id=""([^""]+)""\s+question=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex QueryAgentRegex();

    [GeneratedRegex(@"\[CONFIDENCE:\s*([\d.]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ConfidenceRegex();

    [GeneratedRegex(@"\[REQUEST_FILE:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex FileRequestRegex();

    [GeneratedRegex(@"\[CREATE_AGENT:\s*name=""([^""]+)""\s+expertise=""([^""]+)""\s+files=""([^""]*)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex CreateAgentRegex();

    [GeneratedRegex(@"\[SUGGEST_VALIDATION:\s*expertise=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ValidationRegex();

    [GeneratedRegex(@"\[GENERATE_FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*([\s\S]*?)\[/GENERATE_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedFileRegex();

    [GeneratedRegex(@"\[APPEND_TO_FILE:\s*name=""([^""]+)""\]\s*([\s\S]*?)\[/APPEND_TO_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex AppendToFileRegex();

    [GeneratedRegex(@"\[REQUEST_FILE_PATHS:\s*assembly=""([^""]*)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex FilePathsRequestRegex();

    [GeneratedRegex(@"\[TASK_COMPLETE\]", RegexOptions.IgnoreCase)]
    private static partial Regex TaskCompleteRegex();
}