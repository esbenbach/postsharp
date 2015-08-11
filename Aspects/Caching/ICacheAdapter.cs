
namespace Aspects.Caching
{
    /// <summary>
    /// Represents an adapter to a cache provider
    /// </summary>
    public interface ICacheAdapter
    {
        /// <summary>
        /// Adds an entry with the specified key to the cache in the cache region
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="cacheItemPolicy">The cache item policy.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// true if added false if something with the key already exists
        /// </returns>
        bool AddEntry(string key, object entry, ICacheItemPolicy cacheItemPolicy, string cacheRegion = null);

        /// <summary>
        /// Gets the cache item with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// The entry if it exists otherwise null
        /// </returns>
        object GetEntry(string key, string cacheRegion = null);

        /// <summary>
        /// Determines whether the cache contains and object with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string key, string cacheRegion = null);

        /// <summary>
        /// Deletes the object with the specified key from the cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        void Delete(string key, string cacheRegion = null);

        /// <summary>
        /// Deletes all objects within the specified region.
        /// </summary>
        /// <param name="cacheRegion">The cache region.</param>
        void DeleteRegion(string cacheRegion);
    }
}