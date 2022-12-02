using BoDi;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

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
