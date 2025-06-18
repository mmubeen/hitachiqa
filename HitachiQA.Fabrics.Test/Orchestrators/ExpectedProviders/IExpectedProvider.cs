using Fabrics.Test.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fabrics.Test.Orchestrators.ExpectedProviders
{
    public interface IExpectedProvider
    {
        List<EntityInfo> EntitiyInfos { get; }
    }
}
