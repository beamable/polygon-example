using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("FederationStorage")]
	public class FederationStorage : MongoStorageObject
	{
	}

	public static class FederationStorageExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for FederationStorage
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> FederationStorageDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<FederationStorage>();

		/// <summary>
		/// Gets a MongoDB collection from FederationStorage by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="FederationStorageCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> FederationStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<FederationStorage, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from FederationStorage by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="FederationStorageCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> FederationStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<FederationStorage, TCollection>();
	}
}
