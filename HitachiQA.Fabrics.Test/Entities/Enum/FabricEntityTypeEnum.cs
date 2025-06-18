using Microsoft.Fabric.Api.Core.Models;
using Microsoft.Fabric.Api.DataPipeline.Models;
using Microsoft.Fabric.Api.Lakehouse.Models;
using Microsoft.Fabric.Api.Notebook.Models;
using Microsoft.Fabric.Api.Warehouse.Models;
using FabricEnvironment = Microsoft.Fabric.Api.Environment.Models.Environment;

using System.ComponentModel;

namespace Fabrics.Test.Entities.Enum;

[DefaultValue(None)]
public enum FabricEntityTypeEnum
{
    None,

    [FabricEntity(typeof(Workspace))]
    Workspace,

    [FabricEntity(typeof(DataPipeline))]
    DataPipeline,

    [FabricEntity(typeof(FabricEnvironment))]
    Environment,

    [FabricEntity(typeof(Notebook))]
    Notebook,

    [FabricEntity(typeof(Lakehouse))]
    Lakehouse,

    [FabricEntity(typeof(Warehouse))]
    Warehouse
}
