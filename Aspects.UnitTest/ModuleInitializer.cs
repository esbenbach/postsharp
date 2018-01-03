using Aspects.Caching;
using Aspects.Caching.Implementations;
using PostSharp.Aspects;

namespace Aspects.UnitTest
{
    public static class ModuleInitializer
    {
        [ModuleInitializer(0)]
        public static void InitializeCache()
        {
            CacheAspect.CacheAdapterFactory = () => new RuntimeMemoryCacheAdapter();
            CacheAspect.CacheItemPolicyFactory = () => new RuntimeCacheItemPolicyAdapter();
        }
    }
}
