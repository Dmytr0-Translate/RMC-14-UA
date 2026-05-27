using Content.Shared._Sich.Hunter.Autodoc;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared._RMC14.Medical.Surgery;
using Content.Shared._RMC14.Medical.Surgery.Conditions;
using Content.Shared._RMC14.Medical.Surgery.Steps;
using Content.Server.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Linq;

namespace Content.Server._Sich.Hunter.Autodoc;

public sealed class AutodocHunterSystem : SharedAutodocHunterSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedCMSurgerySystem _surgery = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutodocHunterComponent, AutodocHunterHealMsg>(OnHealMsg);
        SubscribeLocalEvent<AutodocHunterComponent, AutodocHunterSurgeryStepMsg>(OnSurgeryStepMsg);
        SubscribeLocalEvent<AutodocHunterComponent, AutodocSurgeryDoAfterEvent>(OnSurgeryDoAfter);
    }

    private void OnHealMsg(Entity<AutodocHunterComponent> ent, ref AutodocHunterHealMsg args)
    {
        ent.Comp.IsHealing = !ent.Comp.IsHealing;
        Dirty(ent);
        UpdateUiState(ent);
    }

    private void OnSurgeryStepMsg(Entity<AutodocHunterComponent> ent, ref AutodocHunterSurgeryStepMsg args)
    {
        if (!TryComp(ent, out EntityStorageComponent? storage) || storage.Contents.ContainedEntities.Count == 0)
            return;

        var patient = storage.Contents.ContainedEntities[0];

        if (!IsNaked(patient))
        {
            if (args.Actor != EntityUid.Invalid)
                _popup.PopupEntity("Patient must remove armor and uniform!", ent, args.Actor, PopupType.MediumCaution);
            return;
        }

        // args.Target and args.Part are already EntityUid — no GetEntity() needed
        if (!args.Target.IsValid() || !args.Part.IsValid())
            return;

        var patientEnt = args.Target;
        var partEnt = args.Part;

        // 20% slower than normal = 2.4 seconds base
        float duration = 2.4f;

        var ev = new AutodocSurgeryDoAfterEvent(args.Surgery, args.Step, patientEnt, partEnt);
        var doAfter = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(duration), ev, ent, patientEnt)
        {
            BreakOnMove = false,
            TargetEffect = "RMCEffectHealBusy",
            MovementThreshold = 0.5f,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnSurgeryDoAfter(Entity<AutodocHunterComponent> ent, ref AutodocSurgeryDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp(ent, out EntityStorageComponent? storage) || storage.Contents.ContainedEntities.Count == 0)
            return;

        var patient = storage.Contents.ContainedEntities[0];
        if (patient != args.Target)
            return;

        if (!IsNaked(patient))
            return;

        if (_surgery.GetSingleton(args.Step) is not { } stepEnt)
            return;

        // CMSurgeryStepEvent(User, Body, Part, Tools)
        var stepEv = new CMSurgeryStepEvent(ent.Owner, patient, args.Part, new List<EntityUid> { ent.Owner });
        RaiseLocalEvent(stepEnt, ref stepEv);

        args.Handled = true;
    }

    private void UpdateUiState(Entity<AutodocHunterComponent> ent)
    {
        var options = new List<AutodocHunterSurgeryOption>();

        if (TryComp(ent, out EntityStorageComponent? storage) && storage.Contents.ContainedEntities.Count > 0)
        {
            var patient = storage.Contents.ContainedEntities[0];

            foreach (var part in _body.GetBodyChildren(patient))
            {
                foreach (var surgeryProto in _prototypes.EnumeratePrototypes<EntityPrototype>())
                {
                    // Use TryGetComponent instead of HasComponent on EntityPrototype
                    if (!surgeryProto.TryGetComponent<CMSurgeryComponent>(out _))
                        continue;

                    var surgeryId = new EntProtoId(surgeryProto.ID);
                    var surgeryEnt = _surgery.GetSingleton(surgeryId);

                    if (surgeryEnt == null || !TryComp(surgeryEnt, out CMSurgeryComponent? surgeryComp))
                        continue;

                    var nextStep = _surgery.GetNextStep(patient, part.Id, surgeryEnt.Value);
                    if (nextStep != null)
                    {
                        var stepProtoId = nextStep.Value.Surgery.Comp.Steps[nextStep.Value.Step];

                        var ev = new CMSurgeryValidEvent(patient, part.Id);
                        var stepEnt = _surgery.GetSingleton(stepProtoId);
                        if (stepEnt != null)
                        {
                            RaiseLocalEvent(stepEnt.Value, ref ev);
                            RaiseLocalEvent(surgeryEnt.Value, ref ev);

                            if (!ev.Cancelled)
                            {
                                var name = surgeryProto.Name;
                                if (string.IsNullOrEmpty(name)) name = surgeryProto.ID;
                                options.Add(new AutodocHunterSurgeryOption(surgeryId, stepProtoId, part.Id, name));
                            }
                        }
                    }
                }
            }
        }

        _ui.SetUiState(ent, AutodocHunterUiKey.Key, new AutodocHunterBuiState(ent.Comp.IsHealing, options));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AutodocHunterComponent, EntityStorageComponent>();
        while (query.MoveNext(out var uid, out var autodoc, out var storage))
        {
            if (!autodoc.IsHealing)
                continue;

            if (_timing.CurTime < autodoc.NextHealTime)
                continue;

            autodoc.NextHealTime = _timing.CurTime + autodoc.HealInterval;

            if (storage.Contents.ContainedEntities.Count == 0)
            {
                autodoc.IsHealing = false;
                Dirty(uid, autodoc);
                UpdateUiState((uid, autodoc));
                continue;
            }

            var patient = storage.Contents.ContainedEntities[0];

            if (!IsNaked(patient))
                continue;

            if (!TryComp(patient, out DamageableComponent? damageable) || damageable.TotalDamage <= 0)
            {
                autodoc.IsHealing = false;
                Dirty(uid, autodoc);
                UpdateUiState((uid, autodoc));
                continue;
            }

            foreach (var group in damageable.DamagePerGroup)
            {
                if (group.Value > 0)
                {
                    foreach (var type in damageable.Damage.DamageDict)
                    {
                        if (type.Value > 0)
                        {
                            var healAmount = -FixedPoint2.Min(type.Value, FixedPoint2.New(autodoc.HealingAmountPerSecond));
                            var spec = new DamageSpecifier();
                            spec.DamageDict.Add(type.Key, healAmount);
                            _damageable.TryChangeDamage(patient, spec, true, origin: uid);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    private bool IsNaked(EntityUid patient)
    {
        if (_inventory.TryGetSlotEntity(patient, "outerClothing", out _))
            return false;

        if (_inventory.TryGetSlotEntity(patient, "jumpsuit", out _) || _inventory.TryGetSlotEntity(patient, "innerClothing", out _))
            return false;

        if (_inventory.TryGetSlotEntity(patient, "armor", out _))
            return false;

        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class AutodocSurgeryDoAfterEvent : SimpleDoAfterEvent
{
    public EntProtoId Surgery { get; }
    public EntProtoId Step { get; }
    public EntityUid Target { get; }
    public EntityUid Part { get; }

    public AutodocSurgeryDoAfterEvent(EntProtoId surgery, EntProtoId step, EntityUid target, EntityUid part)
    {
        Surgery = surgery;
        Step = step;
        Target = target;
        Part = part;
    }
}