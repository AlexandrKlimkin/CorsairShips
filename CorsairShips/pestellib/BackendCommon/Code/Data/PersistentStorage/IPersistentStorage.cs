using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendCommon.Code.Data.PersistentStorage
{
    public interface IPersistentStorage
    {
        /// <summary>
        /// Saves raw data bound to the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns>false on fail</returns>
        bool SaveRawData(string key, byte[] data);
        /// <summary>
        /// Loads raw data for specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null if data not found</returns>
        byte[] LoadRawData(string key);
        /// <summary>
        /// Removes data bound to the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>false if not found</returns>
        bool Remove(string key);
        /// <summary>
        /// Get specified amount of keys from persistent storage
        /// Full enumeration of resulting IEnumerable (such as .Count(), .ToArray() etc) could cause big memory allocations
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetKeys(int skip, int count, DateTime lastLogin = default(DateTime));
        /// <summary>
        /// Checks if specified key is presented in persistent storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);
        /// <summary>
        /// Amount to KeyValues saved in storage
        /// </summary>
        /// <returns></returns>
        long Count();
        /// <summary>
        /// Amount of Keys saved after specified date.
        /// </summary>
        /// <returns></returns>
        long CountBySaveTime(DateTime dt);
        /// <summary>
        /// Checks if storage conatains specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasKey(string key);

        /// <summary>
        /// Get specific amount of keys filtered by age and data size
        /// </summary>
        /// <param name="maxAge"></param>
        /// <param name="minSize">min size of data stored by key</param>
        /// <param name="count">max amount of keys to return</param>
        /// <returns>array of keys</returns>
        string[] GetKeys(TimeSpan maxAge, int minSize, int count);
        /// <summary>
        /// Gets update time of raw data for specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        DateTime GetTime(string key);

        Task<bool> SaveRawDataAsync(string key, byte[] data);
        Task<byte[]> LoadRawDataAsync(string key);
        Task<bool> RemoveAsync(string key);
    }
}
