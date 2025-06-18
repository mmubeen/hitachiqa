using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public abstract class EntityOrchestratorBase<T> : EntityOrchestratorBase
{
    protected Type EntityType => typeof(T);

    protected EntityOrchestratorBase(IConfiguration config, FabricClient fabricClient) : base(config, fabricClient)
    {

    }

    // ***Typed*** method that real implementations override.
    protected abstract IEnumerable<T> ListItems(Guid workspaceId);

    // Overrides the untyped contract by delegating to the typed one.
    protected override IEnumerable ListItemsGenetic(Guid workspaceId) =>
        ListItems(workspaceId);

    // Optional convenience wrapper that returns IEnumerable<T>.
    public override IEnumerable<T> ListItems() => ListItems(WorkspaceId);
}

public abstract class EntityOrchestratorBase
{

    protected IConfiguration Config { get; init; }
    protected FabricClient FabricClient { get; init; }
    public Guid WorkspaceId { get; init; }

    protected abstract IEnumerable ListItemsGenetic(Guid workspaceId);

    protected EntityOrchestratorBase(IConfiguration config, FabricClient fabricClient)
    {
        Config = config;
        WorkspaceId = Guid.Parse(Config.GetVariable("WORKSPACE_ID"));
        FabricClient = fabricClient;
    }

    public virtual IEnumerable ListItems()
    {
        var items = ListItemsGenetic(WorkspaceId);
        var result = new List<object>();

        foreach (var item in items)
            result.Add(item);

        return result;
    }
}
