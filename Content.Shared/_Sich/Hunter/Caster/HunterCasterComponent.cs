using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Shared._Sich.Hunter.Caster;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HunterCasterComponent : Component
{
    [DataField, AutoNetworkedField]
    public HunterCasterMode CurrentMode = HunterCasterMode.Stun;

    [DataField, AutoNetworkedField]
    public bool Active = false;

    [DataField]
    public float FireDelay = 1.0f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HunterCasterProviderComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionToggleHunterCaster";

    [DataField, AutoNetworkedField]
    public bool Active = false;

    [DataField, AutoNetworkedField]
    public EntityUid? SpawnedCaster;

    public EntityUid? ActionEntity;

    [DataField]
    public SoundSpecifier? SoundOn;

    [DataField]
    public SoundSpecifier? SoundOff;
}

[Serializable, NetSerializable]
public enum HunterCasterMode : byte
{
    Stun,
    Immobilizer,
    Bolt,
    Eradicator
}

[Serializable, NetSerializable]
public enum HunterCasterVisualLayers : byte
{
    Base
}

public sealed partial class HunterCasterActionEvent : InstantActionEvent {}
