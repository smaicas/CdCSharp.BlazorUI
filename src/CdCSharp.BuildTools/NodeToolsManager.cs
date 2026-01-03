using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CdCSharp.BuildTools;

/// <summary>
/// Manages all Node.js based tools (npm, npx, vite, etc.)
/// </summary>
public class NodeToolsManager
{
    private readonly BuildContext _context;

    public NodeToolsManager(BuildContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ensures npm packages are installed
    /// </summary>
    public async Task EnsurePackagesInstalledAsync()
    {
        string nodeModulesPath = _context.GetFullPath("node_modules");

        if (!Directory.Exists(nodeModulesPath))
        {
            Console.WriteLine("    - Installing npm packages...");
            await RunNpmInstallAsync();
        }
    }

    /// <summary>
    /// Builds CSS using Vite
    /// </summary>
    public async Task BuildCssAsync()
    {
        await RunViteBuildAsync("vite.config.css.js");
    }

    /// <summary>
    /// Builds JavaScript/TypeScript using Vite
    /// </summary>
    public async Task BuildJsAsync()
    {
        await RunViteBuildAsync("vite.config.js");
    }

    /// <summary>
    /// Verifies Node.js is installed
    /// </summary>
    public async Task VerifyNodeInstalledAsync()
    {
        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Node.js verification failed");
            }
        }
        catch
        {
            throw new InvalidOperationException(
                "Node.js is required to build BlazorUI. Please install Node.js from https://nodejs.org/");
        }
    }

    private async Task RunNpmInstallAsync()
    {
        await RunCommand(GetNpmPath(), "install", _context.ProjectPath);
    }

    private async Task RunViteBuildAsync(string configFile)
    {
        string args = "vite build";
        if (!string.IsNullOrWhiteSpace(configFile))
        {
            args += $" --config {configFile}";
        }

        await RunCommand(GetNpxPath(), args, _context.ProjectPath);
    }

    private static string GetNpmPath()
    {
        return TryGetSystemCommand("npm", out string systemNpm)
            ? systemNpm
            : throw new InvalidOperationException(
                "Node.js/npm not found. Please install Node.js from https://nodejs.org/");
    }

    private static string GetNpxPath()
    {
        return TryGetSystemCommand("npx", out string systemNpx)
            ? systemNpx
            : throw new InvalidOperationException(
                "npx not found. Please install Node.js from https://nodejs.org/");
    }

    private static async Task RunCommand(string fileName, string arguments, string workingDirectory)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = startInfo };

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine(e.Data);
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}. Error: {error}");
        }
    }

    private static bool TryGetSystemCommand(string command, out string commandPath)
    {
        commandPath = command;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Buscar node.exe primero
            try
            {
                ProcessStartInfo whereInfo = new()
                {
                    FileName = "where",
                    Arguments = "node.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using Process whereProcess = Process.Start(whereInfo);
                if (whereProcess != null)
                {
                    string output = whereProcess.StandardOutput.ReadToEnd().Trim();
                    whereProcess.WaitForExit();

                    if (whereProcess.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        string nodePath = output.Split('\n')[0].Trim();
                        Console.WriteLine($"Found node.exe at: {nodePath}");

                        // npm.cmd y npx.cmd están en el mismo directorio que node.exe
                        string nodeDir = Path.GetDirectoryName(nodePath);
                        commandPath = Path.Combine(nodeDir, $"{command}.cmd");

                        if (File.Exists(commandPath))
                        {
                            Console.WriteLine($"Found {command}.cmd at: {commandPath}");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding node: {ex.Message}");
            }

            // Fallback: intentar directamente con .cmd
            commandPath = $"{command}.cmd";
        }

        // Verificar que funciona
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = commandPath,
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Verified {command} version: {output.Trim()}");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run {commandPath}: {ex.Message}");
        }

        commandPath = null;
        return false;
    }
}