// AI/RetryPolicy.cs
using CdCSharp.DocGen.Core.Models.AI;
using Microsoft.Extensions.Logging;

namespace CdCSharp.DocGen.Core.AI;

public class RetryPolicy
{
    private readonly ILogger _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryPolicy(ILogger logger, int maxRetries = 3, int baseDelaySeconds = 2)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _baseDelay = TimeSpan.FromSeconds(baseDelaySeconds);
    }

    public async Task<AiResponse> ExecuteAsync(
        Func<Task<AiResponse>> operation,
        CancellationToken cancellationToken = default)
    {
        AiResponse lastResponse = AiResponse.Fail(AiErrorType.Unknown, "No attempts made");

        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return AiResponse.Fail(AiErrorType.Unknown, "Operation cancelled");
            }

            try
            {
                lastResponse = await operation();

                if (lastResponse.Success)
                {
                    return lastResponse;
                }

                if (lastResponse.Error?.Type == AiErrorType.RateLimit)
                {
                    TimeSpan delay = CalculateDelay(attempt, isRateLimit: true);
                    _logger.LogWarning("Rate limit hit, waiting {Delay}s before retry {Attempt}/{Max}",
                        delay.TotalSeconds, attempt + 1, _maxRetries);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                if (lastResponse.Error?.Type is AiErrorType.Timeout or
                    AiErrorType.ConnectionError)
                {
                    TimeSpan delay = CalculateDelay(attempt, isRateLimit: false);
                    _logger.LogWarning("Transient error ({Type}), waiting {Delay}s before retry {Attempt}/{Max}",
                        lastResponse.Error.Type, delay.TotalSeconds, attempt + 1, _maxRetries);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                return lastResponse;
            }
            catch (OperationCanceledException)
            {
                return AiResponse.Fail(AiErrorType.Unknown, "Operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error on attempt {Attempt}/{Max}", attempt + 1, _maxRetries);
                lastResponse = AiResponse.Fail(AiErrorType.Unknown, ex.Message);

                if (attempt < _maxRetries - 1)
                {
                    TimeSpan delay = CalculateDelay(attempt, isRateLimit: false);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        _logger.LogError("All {Max} retry attempts failed", _maxRetries);
        return lastResponse;
    }

    private TimeSpan CalculateDelay(int attempt, bool isRateLimit)
    {
        double multiplier = isRateLimit ? 30 : Math.Pow(2, attempt);
        return TimeSpan.FromSeconds(_baseDelay.TotalSeconds * multiplier);
    }
}