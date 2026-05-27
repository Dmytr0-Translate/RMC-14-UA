using Robust.Shared.GameStates;

namespace Content.Shared._Sich.Hunter.Autodoc;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AutodocHunterComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsHealing;

    [DataField, AutoNetworkedField]
    public float HealingAmountPerSecond = 0.83f; // ~100 damage over 2 minutes

    [DataField]
    public TimeSpan NextHealTime;

    [DataField]
    public TimeSpan HealInterval = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Multiplier for the duration of the surgery step DoAfter.
    /// User requested 20% slower, so 1.2f.
    /// </summary>
    [DataField]
    public float SurgerySpeedMultiplier = 1.2f;
}
