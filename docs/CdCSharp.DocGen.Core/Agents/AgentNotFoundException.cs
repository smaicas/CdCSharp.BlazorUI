namespace CdCSharp.DocGen.Core.Agents;

internal class AgentNotFoundException : InvalidOperationException
{
    public AgentNotFoundException(string message) : base(message) { }
}
