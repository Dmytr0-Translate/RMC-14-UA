using Content.Shared._Sich.Hunter.Autodoc;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Sich.Hunter.Autodoc.UI;

[UsedImplicitly]
public sealed class AutodocHunterBoundUserInterface : BoundUserInterface
{
    private AutodocHunterWindow? _window;

    public AutodocHunterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new AutodocHunterWindow();
        
        _window.OnClose += Close;
        _window.OnAutoHealPressed += () => SendMessage(new AutodocHunterHealMsg());
        _window.OnSurgeryStepPressed += (surgery, step, part) => SendMessage(new AutodocHunterSurgeryStepMsg(surgery, step, part, EntMan.GetNetEntity(Owner)));

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AutodocHunterBuiState castState)
            return;

        _window?.UpdateState(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _window?.Dispose();
    }
}
