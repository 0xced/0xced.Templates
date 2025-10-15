using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace myapp;

[Description("Waits for some time.")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli through reflection")]
internal class DefaultCommand(IAnsiConsole console) : AsyncCommand<DefaultCommand.Settings>
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Required for Spectre.Console.Cli binding")]
    internal class Settings : CommandSettings
    {
        [Description("Time to wait before completing.")]
        [CommandOption("-w|--wait")]
        [DefaultValue("00:00:02")]
        public TimeSpan Delay { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext commandContext, Settings settings, CancellationToken cancellationToken)
    {
        return await console.Status().Spinner(Spinner.Known.Clock).StartAsync("Waiting", async context =>
        {
            context.Status = $"Waiting for {settings.Delay.TotalSeconds} seconds";
            console.WriteLine("Started");
            await Task.Delay(settings.Delay, cancellationToken);
            console.WriteLine("✅ Done");
            return 0;
        });
    }
}