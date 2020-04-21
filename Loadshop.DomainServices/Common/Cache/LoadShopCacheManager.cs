namespace Loadshop.DomainServices.Common.Cache
{
    public class LoadShopCacheManager
    {
        public UserCacheManager UserCache { get; }
        
        public LoadShopCacheManager(UserCacheManager userCache)
        {
            UserCache = userCache;
        }
    }
}
