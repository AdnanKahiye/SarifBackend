using Backend.Wrapper;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Backend.Utiliy
{
    public abstract class CacheService
    {
        protected readonly IMemoryCache _cache;

        // ✅ MUHIIM: Waxaan ka dhignay 'static' si keys-ka aysan u lumin 
        // marka UserService cusub la abuuro (Request kasta).
        // Waxaan u isticmaalay ConcurrentBag si uu 'Thread-safe' u noqdo.
        private static readonly ConcurrentBag<string> _cacheKeys = new ConcurrentBag<string>();

        protected CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Wuxuu ka soo ridaa xogta Cache-ga, haddii kalena Database-ka ayuu ka keenayaa oo kaydinayaa.
        /// </summary>
        protected async Task<ResponseWrapper<T>> ExecuteWithCacheAsync<T>(
            string cacheKey,
            Func<Task<T>> action,
            Func<T, string> successMessageFactory,
            string cacheMessage,
            string errorMessage,
            int cacheMinutes = 5)
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out T cachedResult))
                {
                    return await ResponseWrapper<T>.SuccessAsync(cachedResult, cacheMessage);
                }

                var result = await action();
                string successMessage = successMessageFactory(result);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheMinutes))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, result, cacheOptions);
                AddCacheKey(cacheKey);

                return await ResponseWrapper<T>.SuccessAsync(result, successMessage);
            }
            catch (Exception ex)
            {
                return await ResponseWrapper<T>.FailureAsync($"{errorMessage}: {ex.Message}");
            }
        }

        /// <summary>
        /// Wuxuu fuliyaa hawlaha qoraalka (Update/Delete) wuxuuna tirtiraa dhamaan Cache-ga.
        /// </summary>
        protected async Task<ResponseWrapper<T>> ExecuteWriteAsync<T>(
            Func<Task<T>> action,
            string successMessage,
            string errorMessage)
        {
            try
            {
                var result = await action();

                // Mar kasta oo wax la qoro (Delete/Update), tirtir cache-ga si xogta loo cusubaysiiyo
                ClearAllCache();

                return await ResponseWrapper<T>.SuccessAsync(result, successMessage);
            }
            catch (Exception ex)
            {
                // Halkan waxaa lagu qabanayaa 'throw new Exception' kasta oo ka yimid UserService
                return await ResponseWrapper<T>.FailureAsync($"{errorMessage}: {ex.Message}");
            }
        }

        // --- CACHE HELPERS ---

        protected void AddCacheKey(string key)
        {
            if (!_cacheKeys.Contains(key))
            {
                _cacheKeys.Add(key);
            }
        }

        /// <summary>
        /// Wuxuu tirtiraa xogta ku bilaabata prefix gaar ah (tusaale: "Users")
        /// </summary>
        public void RemoveByPrefix(string prefix)
        {
            // Ka raadi prefix-ka liiska static-ga ah
            var keysToRemove = _cacheKeys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);

                // Ka saar liiska aynu track-ga ku hayno
                _cacheKeys.TryTake(out _);
            }
        }

        protected void ClearAllCache()
        {
            foreach (var key in _cacheKeys)
            {
                _cache.Remove(key);
            }

            // Faaruqi liiska keys-ka
            while (!_cacheKeys.IsEmpty)
            {
                _cacheKeys.TryTake(out _);
            }
        }

        protected void RemoveCacheKey(string key)
        {
            _cache.Remove(key);
        }
    }
}