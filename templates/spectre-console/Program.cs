using myapp;
using System;
using System.Reflection;
using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DefaultCommand>();
var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    // Try to cancel gracefully the first time, then abort the process the second time Ctrl+C is pressed
    eventArgs.Cancel = !cancellationTokenSource.IsCancellationRequested;
    cancellationTokenSource.Cancel();
};

app.Configure(config =>
{
    var assembly = typeof(Program).Assembly;
    var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "N/A";
    config.SetApplicationVersion(version);
    config.ConfigureConsole(RedirectionFriendlyConsole.Out);
    config.SetExceptionHandler((exception, _) =>
    {
        switch (exception)
        {
            case OperationCanceledException when cancellationTokenSource.IsCancellationRequested:
                return config.Settings.CancellationExitCode;
            case CommandAppException { Pretty: not null } commandAppException:
                RedirectionFriendlyConsole.Error.Write(commandAppException.Pretty);
                break;
            case CommandAppException commandAppException:
                RedirectionFriendlyConsole.Error.WriteLine(commandAppException.Message, Color.Red);
                break;
            default:
                RedirectionFriendlyConsole.Error.WriteException(exception, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes);
                break;
        }

        if (exception is CommandAppException)
        {
            app.Run(["--help"]);
            return 64; // EX_USAGE -- The command was used incorrectly, e.g., with the wrong number of arguments, a bad flag, a bad syntax in a parameter, or whatever.
        }

        return 70; // EX_SOFTWARE -- An internal software error has been detected.
    });
});

return await app.RunAsync(args, cancellationTokenSource.Token);