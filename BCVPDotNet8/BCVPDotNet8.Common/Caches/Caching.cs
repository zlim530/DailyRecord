using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace BCVPDotNet8.Common.Caches
{
    public class Caching : ICaching
    {
        private readonly IDistributedCache _cache;

        public Caching(IDistributedCache cache)
        {
            _cache = cache;
        }

        private byte[] GetBytes<T>(T source)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source));
        }


        public IDistributedCache Cache => _cache;

        public void AddCacheKey(string cacheKey)
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (!allKeys.Any(m => m == cacheKey))
            {
                allKeys.Add(cacheKey);
                _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }

        }

        /// <summary>
        /// 增加缓存Key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task AddCacheKeyAsync(string cacheKey)
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (!allKeys.Any(m => m == cacheKey))
            {
                allKeys.Add(cacheKey);
                await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }

        }

        /// <summary>
        /// 根据父键清空
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DelByParentKeyAsync(string key)
        {
            var allKeys = await GetAllCacheKeysAsync();
            if (allKeys == null)
            {
                return;
            }

            var delAllkeys = allKeys.Where(u => u.StartsWith(key)).ToList();
            delAllkeys.ForEach(Remove);
            // 更新所有缓存键
            allKeys = allKeys.Where(u => !u.StartsWith(key)).ToList();
            await SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));

        }

        public void DelByPattern(string key)
        {
            var allKeys = GetAllCacheKeys();
            if (allKeys == null)
            {
                return;
            }

            var delAllkeys = allKeys.Where(u => u.Contains(key)).ToList();
            delAllkeys.ForEach(u => { _cache.Remove(u); });

            // 更新所有缓存键
            allKeys = allKeys.Where(u => !u.Contains(key)).ToList();
            _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
        }

        /// <summary>
        /// 删除某特征关键字缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DelByPatternAsync(string key)
        {
            var allkeys = await GetAllCacheKeysAsync();
            if (allkeys == null) return;

            var delAllkeys = allkeys.Where(u => u.Contains(key)).ToList();
            delAllkeys.ForEach(u => { _cache.Remove(u); });

            // 更新所有缓存键
            allkeys = allkeys.Where(u => !u.Contains(key)).ToList();
            await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allkeys));
        }

        public void DelCacheKey(string cacheKey)
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            var allkeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (allkeys.Any(m => m == cacheKey))
            {
                allkeys.Remove(cacheKey);
                _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allkeys));
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task DelCacheKeyAsync(string cacheKey)
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            var allkeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (allkeys.Any(m => m == cacheKey))
            {
                allkeys.Remove(cacheKey);
                await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allkeys));
            }
        }

        public bool Exists(string cacheKey)
        {
            var res = _cache.Get(cacheKey);
            return res != null;
        }

        /// <summary>
        /// 检查给定 key 是否存在
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            var res = await _cache.GetAsync(cacheKey);
            return res != null;
        }

        public T Get<T>(string cacheKey)
        {
            var res = _cache.Get(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
        }

        public object Get(Type type, string cacheKey)
        {
            var res = _cache.Get(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
        }

        public List<string> GetAllCacheKeys()
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            return string.IsNullOrWhiteSpace(res) ? null : JsonConvert.DeserializeObject<List<string>>(res);
        }

        /// <summary>
        /// 获取所有缓存列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllCacheKeysAsync()
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            return string.IsNullOrWhiteSpace(res) ? null : JsonConvert.DeserializeObject<List<string>>(res);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var res = await _cache.GetAsync(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
        }


        public async Task<object> GetASync(Type type, string cacheKey)
        {
            var res = await _cache.GetAsync(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
        }

        public string GetString(string key)
        {
            return _cache.GetString(key);
        }

        public async Task<string> GetStringAsync(string cacheKey)
        {
            return await _cache.GetStringAsync(cacheKey);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            DelCacheKey(key);
        }

        public void RemoveAll()
        {
            var catches = GetAllCacheKeys();
            foreach (var @catch in catches) Remove(@catch);

            catches.Clear();
            _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(catches));
        }

        public async Task RemoveAllAsync()
        {
            var catches = await GetAllCacheKeysAsync();
            foreach (var @catch in catches) await RemoveAsync(@catch);

            catches.Clear();
            await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(catches));
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            await DelCacheKeyAsync(key);
        }

        public void Set<T>(string cacheKey, T value, TimeSpan? expire = null)
        {
            _cache.Set(cacheKey, GetBytes(value),
               expire == null
                   ? new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) }
                   : new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });

            AddCacheKey(cacheKey);
        }

        /// <summary>
        /// 增加对象缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetAsync<T>(string cacheKey, T value)
        {
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
               new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });

            await AddCacheKeyAsync(cacheKey);
        }

        /// <summary>
        /// 增加对象缓存,并设置过期时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expire)
        {
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
                 new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });

            await AddCacheKeyAsync(cacheKey);
        }

        public void SetParmanent<T>(string cacheKey, T value)
        {
            _cache.Set(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
            AddCacheKey(cacheKey);
        }

        public async Task SetPermanentAsync<T>(string cacheKey, T value)
        {
            await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
            await AddCacheKeyAsync(cacheKey);
        }

        public void SetString(string cacheKey, string value, TimeSpan? expire = null)
        {
            if (expire == null)
                _cache.SetString(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });
            else
                _cache.SetString(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });

            AddCacheKey(cacheKey);
        }

        /// <summary>
        /// 增加字符串缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetStringAsync(string cacheKey, string value)
        {
            await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });

            await AddCacheKeyAsync(cacheKey);
        }

        public async Task SetStringAsync(string cacheKey, string value, TimeSpan expire)
        {
            await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });

            await AddCacheKeyAsync(cacheKey);
        }
    }
}
