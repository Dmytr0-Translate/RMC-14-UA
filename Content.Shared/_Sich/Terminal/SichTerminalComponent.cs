using System.Collections.Generic;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Access;

namespace Content.Shared._Sich.Terminal;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSichTerminalSystem))]
public sealed partial class SichTerminalComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsInput = false;

    [DataField, AutoNetworkedField]
    public List<string> Messages = new();

    [DataField, AutoNetworkedField]
    public string? AuthorizedName;

    [DataField, AutoNetworkedField]
    public List<ProtoId<AccessLevelPrototype>> RequiredAccesses = new();

    [DataField]
    public string IdCardSlotId = "id_card";

    [DataField]
    public SoundSpecifier ClickSound = new SoundCollectionSpecifier("Keyboard");

    [DataField]
    public SoundSpecifier AdminMessageSound = new SoundPathSpecifier("/Audio/_Sich/admin_message.ogg");
}
