using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("PolygonStorage")]
	public class PolygonStorage : MongoStorageObject
	{
	}

	public static class PolygonStorageExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for PolygonStorage
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> PolygonStorageDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<PolygonStorage>();

		/// <summary>
		/// Gets a MongoDB collection from PolygonStorage by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="PolygonStorageCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> PolygonStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<PolygonStorage, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from PolygonStorage by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="PolygonStorageCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> PolygonStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<PolygonStorage, TCollection>();
	}
}
