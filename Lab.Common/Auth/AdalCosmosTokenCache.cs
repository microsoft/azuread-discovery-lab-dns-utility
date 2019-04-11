using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DocDBLib;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Infra.Auth
{
    public class CacheUser
    {
        public string UserObjId { get; set; }
        public string HostName { get; set; }
        public CacheUser(string userObjId, string hostName)
        {
            UserObjId = userObjId;
            HostName = hostName;
        }
        public CacheUser(HttpContextBase hctx)
        {
            UserObjId = hctx.User.Identity.GetClaim(TokenCacheClaimTypes.ObjectId);
            HostName = Utils.GetFQDN(hctx.Request);
        }
    }

    public class PerWebUserCache : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "webUserUniqueId")]
        public string WebUserUniqueId { get; set; }

        [JsonProperty(PropertyName = "cacheBits")]
        public byte[] CacheBits { get; set; }

        [JsonProperty(PropertyName = "lastWrite")]
        public DateTime LastWrite { get; set; }

        [JsonProperty(PropertyName = "hostName")]
        public string HostName { get; set; }

        [JsonProperty(PropertyName = "salt")]
        public byte[] Salt { get; set; }

        public static async Task<PerWebUserCache> GetCache(CacheUser user)
        {
            var res = await DocDBRepo.DB<PerWebUserCache>.GetItemsAsync(u => u.WebUserUniqueId == user.UserObjId && u.HostName == user.HostName).ConfigureAwait(false);
            return res.SingleOrDefault();
        }
        public static async Task<PerWebUserCache> AddEntry(PerWebUserCache cache)
        {
            return (await DocDBRepo.DB<PerWebUserCache>.CreateItemAsync(cache).ConfigureAwait(false));
        }
        public static async Task<PerWebUserCache> UpdateEntry(PerWebUserCache cache)
        {
            return (await DocDBRepo.DB<PerWebUserCache>.UpdateItemAsync(cache).ConfigureAwait(false));
        }
        public static async Task<IEnumerable<PerWebUserCache>> GetAllEntries()
        {
            return (await DocDBRepo.DB<PerWebUserCache>.GetItemsAsync().ConfigureAwait(false));
        }
        public static async Task<IEnumerable<PerWebUserCache>> GetAllEntries(CacheUser user)
        {
            return (await DocDBRepo.DB<PerWebUserCache>.GetItemsAsync(u => u.WebUserUniqueId == user.UserObjId && u.HostName == user.HostName).ConfigureAwait(false));
        }
        public static async Task RemoveEntry(PerWebUserCache cache)
        {
            await DocDBRepo.DB<PerWebUserCache>.DeleteItemAsync(cache).ConfigureAwait(false);
        }
        public static async Task RemoveEntry(CacheUser user)
        {
            var cache = await GetCache(user);
            await RemoveEntry(cache);
        }
    }

    public class AdalCosmosTokenCache : TokenCache
    {
        private string _userObjId;
        private string _hostName;
        private PerWebUserCache Cache;

        public AdalCosmosTokenCache(CacheUser user) : this(user.UserObjId, user.HostName)
        {
            
        }

        // constructor
        public AdalCosmosTokenCache(string userObjId, string hostName)
        {
            // associate the cache to the current user of the web app
            _userObjId = userObjId;
            _hostName = hostName;

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            // look up the entry in the DB
            var task = Task.Run(async () => {
                Cache = await PerWebUserCache.GetCache(new CacheUser(_userObjId, _hostName));
            });
            task.Wait();

            try
            {
                // place the entry in memory
                this.Deserialize((Cache == null) ? null : Utils.Decrypt(new EncryptedObj(Cache.CacheBits, Cache.Salt)));
            }
            catch(CryptographicException)
            {
                //token is invalid for decryption - delete it and start fresh
                DeleteItem(Cache);
                this.Deserialize(null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
       // clean up the DB
        public override void Clear()
        {
            base.Clear();
            DeleteItem(Cache);
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        async void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (Cache == null)
            {
                // first time access
                Cache = await PerWebUserCache.GetCache(new CacheUser(_userObjId, _hostName));
            }
            else
            {
                // retrieve last write from the DB
                var dbCache = await PerWebUserCache.GetCache(new CacheUser(_userObjId, _hostName));
                if (dbCache == null)
                {
                    Cache = await PerWebUserCache.GetCache(new CacheUser(_userObjId, _hostName));
                }
                else
                {
                    // if the in-memory copy is older than the persistent copy
                    if (dbCache.LastWrite > Cache.LastWrite)
                    {
                        // update in-memory copy
                        Cache = dbCache;
                    }
                }
            }
            this.Deserialize((Cache == null) ? null : Utils.Decrypt(new EncryptedObj(Cache.CacheBits, Cache.Salt)));
        }
        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        async void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            //Task task;
            // if state changed
            if (this.HasStateChanged)
            {
                var enc = Utils.Encrypt(this.Serialize());

                if (Cache != null)
                {
                    Cache.CacheBits = enc.EncryptedData;
                    Cache.Salt = enc.VectorData;
                    Cache.LastWrite = DateTime.Now;
                    // update the DB and the lastwrite             
                    await PerWebUserCache.UpdateEntry(Cache);

                }
                else
                {
                    Cache = new PerWebUserCache
                    {
                        WebUserUniqueId = _userObjId,
                        CacheBits = enc.EncryptedData,
                        Salt = enc.VectorData,
                        LastWrite = DateTime.Now,
                        HostName = _hostName
                    };
                     await PerWebUserCache.AddEntry(Cache);
                }

                this.HasStateChanged = false;
            }
        }

        public static async Task DeleteItem(HttpContextBase hctx)
        {
            await PerWebUserCache.RemoveEntry(new CacheUser(hctx));
        }

        public static void DeleteItem(PerWebUserCache cache)
        {
            var task = Task.Run(async () => {
                await PerWebUserCache.RemoveEntry(cache);
            });
            task.Wait();
        }

        /// <summary>
        /// Remove all ADAL cache entries from the database
        /// </summary>
        /// <returns></returns>
        public static async Task FlushAllCache()
        {
            IEnumerable<PerWebUserCache> entries = null;
            entries = await PerWebUserCache.GetAllEntries();

            foreach (var cacheEntry in entries)
                await PerWebUserCache.RemoveEntry(cacheEntry);

        }
    }
}