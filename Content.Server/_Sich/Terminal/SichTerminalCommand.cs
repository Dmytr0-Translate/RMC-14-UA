using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Sich.Terminal;

[AdminCommand(AdminFlags.Admin)]
public sealed class SichTerminalCommand : IConsoleCommand
{
    public string Command => "terminalmsg";
    public string Description => "Відправляє повідомлення до терміналу комунікації";
    public string Help => "terminalmsg <повідомлення>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            shell.WriteError("Введіть повідомлення");
            return;
        }

        var message = string.Join(" ", args);
        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SichTerminalSystem>();
        
        system.BroadcastMessage(message);
        shell.WriteLine($"Буде відправлено повідомлення: {message}");
    }
}
