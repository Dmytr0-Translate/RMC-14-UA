using Content.Shared._Sich.Terminal;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client._Sich.Terminal;

[UsedImplicitly]
public sealed class SichTerminalBoundUserInterface : BoundUserInterface
{
    private SichTerminalWindow? _window;

    public SichTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new SichTerminalWindow();
        _window.OnClose += Close;
        _window.OnMessageEntered += msg =>
        {
            SendMessage(new SichTerminalSendMessage(msg));
        };
        
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SichTerminalState terminalState)
            return;

        _window?.SetState(terminalState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
