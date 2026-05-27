using Content.Shared.ActionBlocker;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Lock;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._Sich.Hunter.Autodoc;

public abstract class SharedAutodocHunterSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<AutodocHunterComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<AutodocHunterComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(Entity<AutodocHunterComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != SharedEntityStorageSystem.ContainerName)
            return;
            
        // Reset healing state when someone enters
        ent.Comp.IsHealing = false;
        Dirty(ent);
    }

    private void OnRemoved(Entity<AutodocHunterComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != SharedEntityStorageSystem.ContainerName)
            return;
            
        // Turn off healing when they leave
        ent.Comp.IsHealing = false;
        Dirty(ent);
    }
}
