using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;

namespace HitachiQA.Hooks
{
    public class HookBase
    {
        protected IObjectContainer ObjectContainer;
        protected FeatureContext FeatureContext;
        protected IConfiguration Configuration;
        public HookBase(IObjectContainer ojectContainer, FeatureContext fc, IConfiguration config)
        {
            ObjectContainer = ojectContainer;
            FeatureContext = fc;
            Configuration = config;

        }
    }
}
