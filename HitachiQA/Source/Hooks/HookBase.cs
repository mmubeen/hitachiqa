using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;

namespace HitachiQA.Hooks
{
    public class HookBase
    {
        protected IConfiguration Configuration;
        public HookBase(IConfiguration config)
        {
            Configuration = config;
        }
    }
}
