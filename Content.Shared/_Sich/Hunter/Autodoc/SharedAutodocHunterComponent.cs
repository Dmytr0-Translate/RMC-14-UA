using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.DoAfter;

namespace Content.Shared._Sich.Hunter.Autodoc;

[Serializable, NetSerializable]
public enum AutodocHunterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AutodocHunterHealMsg : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class AutodocHunterSurgeryStepMsg : BoundUserInterfaceMessage
{
    public EntProtoId Surgery { get; }
    public EntProtoId Step { get; }
    public NetEntity Part { get; }
    public NetEntity Target { get; }

    public AutodocHunterSurgeryStepMsg(EntProtoId surgery, EntProtoId step, NetEntity part, NetEntity target)
    {
        Surgery = surgery;
        Step = step;
        Part = part;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class AutodocHunterSurgeryOption
{
    public EntProtoId Surgery;
    public EntProtoId Step;
    public NetEntity Part;
    public string Name;

    public AutodocHunterSurgeryOption(EntProtoId surgery, EntProtoId step, NetEntity part, string name)
    {
        Surgery = surgery;
        Step = step;
        Part = part;
        Name = name;
    }
}

[Serializable, NetSerializable]
public sealed class AutodocHunterBuiState : BoundUserInterfaceState
{
    public bool IsHealing { get; }
    public List<AutodocHunterSurgeryOption> SurgeryOptions { get; }
    
    public AutodocHunterBuiState(bool isHealing, List<AutodocHunterSurgeryOption> surgeryOptions)
    {
        IsHealing = isHealing;
        SurgeryOptions = surgeryOptions;
    }
}

[Serializable, NetSerializable]
public sealed partial class AutodocSurgeryDoAfterEvent : SimpleDoAfterEvent
{
    public EntProtoId Surgery { get; }
    public EntProtoId Step { get; }
    public NetEntity Target { get; }
    public NetEntity Part { get; }

    public AutodocSurgeryDoAfterEvent(EntProtoId surgery, EntProtoId step, NetEntity target, NetEntity part)
    {
        Surgery = surgery;
        Step = step;
        Target = target;
        Part = part;
    }
}
