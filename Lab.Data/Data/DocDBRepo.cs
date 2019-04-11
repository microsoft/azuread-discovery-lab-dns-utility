using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace DocDBLib
{
    public static partial class DocDBRepo
    {
        private static DocumentClient client;
        private static Uri baseDocCollectionUri;

        public static class Settings
        {
            private static string _currRegion;
            private static string[] _regions;

            public static string DocDBUri;
            public static string DocDBAuthKey;
            public static string DocDBName;
            public static string DocDBCollection;
            public static string AppRootPath;

            public static string[] DocDBRegions
            {
                get
                {
                    return _regions.Except(new[] { _currRegion }).ToArray();
                }
                set
                {
                    _regions = value;
                }
            }

            public static string DocDBCurrentRegion
            {
                get
                {
                    return _currRegion;
                }
                set
                {
                    //if running locally with no environment variable, grab the first assigned region from the collection
                    _currRegion = value ?? DocDBRegions[0];
                }
            }
        }

        public static partial class DB<T> where T : class, IDocModelBase
        {
            public static async Task<T> CreateItemAsync(T item)
            {
                item.DocType = typeof(T).Name;
                item.Id = Guid.NewGuid().ToString();
                var res = await client.CreateDocumentAsync(baseDocCollectionUri, item);
                var doc = JsonConvert.DeserializeObject<T>(res.Resource.ToString());
                return doc;
            }

            public static async Task<T> UpdateItemAsync(T item)
            {
                item.DocType = typeof(T).Name;
                var id = item.Id;
                var res = await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Settings.DocDBName, Settings.DocDBCollection, id), item);
                var doc = JsonConvert.DeserializeObject<T>(res.Resource.ToString());
                return doc;
            }

            public static async Task<dynamic> DeleteItemAsync(T item)
            {
                var id = item.Id;
                return await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Settings.DocDBName, Settings.DocDBCollection, id));
            }

            public static async Task<int> DeleteAllItemsAsync()
            {
                int recsDeleted = 0;

                var docType = typeof(T).Name;
                Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);

                var query = client.CreateDocumentQuery<T>(baseDocCollectionUri)
                    .Where(docTypeFilter)
                    .AsDocumentQuery();
                var results = await GetItemsAsync(query, 0);

                foreach (var doc in results)
                {
                    recsDeleted++;
                    await DeleteItemAsync(doc);
                }
                return recsDeleted;
            }

            public static async Task<T> GetItemAsync(string id)
            {
                try
                {
                    Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Settings.DocDBName, Settings.DocDBCollection, id));
                    return (T)(dynamic)document;
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            public static async Task<IEnumerable<T>> GetItemsAsync(IDocumentQuery<T> query)
            {
                return await GetItemsAsync(query, 0);
            }


            public static async Task<IEnumerable<T>> GetItemsAsync(IDocumentQuery<T> query, int returnTop)
            {
                List<T> results = new List<T>();
                try
                {
                    while (query.HasMoreResults && (returnTop == 0 || results.Count < returnTop))
                    {
                        results.AddRange(await query.ExecuteNextAsync<T>());
                    }

                    return (returnTop == 0) ? results : results.Take(returnTop);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
            {
                return await GetItemsAsync(predicate, 0);
            }

            /// <summary>
            /// Returns all documents of type T where (predicate)
            /// </summary>
            /// <param name="predicate"></param>
            /// <param name="returnTop"></param>
            /// <returns></returns>
            public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, int returnTop)
            {
                if (predicate == null) return await GetItemsAsync();

                var docType = typeof(T).Name;

                var cli = client.CreateDocumentQuery<T>(baseDocCollectionUri);
                var cli2 = cli.Where(predicate);

                Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);
                docTypeFilter.Compile();

                var query = cli2.Where(docTypeFilter)
                    .AsDocumentQuery();
                return await GetItemsAsync(query, returnTop);
            }

            /// <summary>
            /// returns all documents of type T
            /// </summary>
            /// <returns></returns>
            public static async Task<IEnumerable<T>> GetItemsAsync(int returnTop = 0)
            {
                var docType = typeof(T).Name;
                Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);

                var query = client.CreateDocumentQuery<T>(baseDocCollectionUri)
                    .Where(docTypeFilter)
                    .AsDocumentQuery();
                return await GetItemsAsync(query, 0);
            }


            public static async Task<IEnumerable<dynamic>> GetAllItemsGenericAsync()
            {
                var docType = typeof(T).Name;

                var query = client.CreateDocumentQuery<IDocModelBase>(baseDocCollectionUri)
                    .Where(d => d.DocType == docType)
                    .AsDocumentQuery();

                List<dynamic> results = new List<dynamic>();
                try
                {
                    while (query.HasMoreResults)
                    {
                        results.AddRange(await query.ExecuteNextAsync<dynamic>());
                    }

                    return results;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public static IOrderedQueryable<T> BuildFromBaseQuery()
            {
                var docType = typeof(T).Name;
                var cli = client.CreateDocumentQuery<T>(baseDocCollectionUri);
                return cli;
            }

            public static async Task AddSPUpdateAssignment(string spName)
            {
                var updateAssignment = new StoredProcedure
                {
                    Id = spName,
                    Body = GetSPText(spName + ".js")
                };

                StoredProcedure createdStoredProcedure = await client.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection), updateAssignment);
            }

            private static string GetSPText(string fileName)
            {
                var path = Path.Combine(Settings.AppRootPath, "bin\\Data\\Procedures\\", fileName);
                var contents = File.ReadAllText(path);
                return contents;
            }

        }
        public static async Task DropAndResetCollection()
        {
            await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection));
            await Initialize();
        }

        public static async Task<DocumentClient> Initialize()
        {
            baseDocCollectionUri = UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection);

            //https://docs.microsoft.com/en-us/azure/cosmos-db/regional-failover
            ConnectionPolicy connPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            connPolicy.EnableEndpointDiscovery = true;
            connPolicy.PreferredLocations.Add(Settings.DocDBCurrentRegion);
            foreach (var region in Settings.DocDBRegions)
            {
                connPolicy.PreferredLocations.Add(region);
            }

            client = new DocumentClient(new Uri(Settings.DocDBUri), Settings.DocDBAuthKey, connPolicy);

            await CreateDatabaseIfNotExistsAsync();
            await CreateCollectionIfNotExistsAsync();

            return client;
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(Settings.DocDBName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = Settings.DocDBName });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(baseDocCollectionUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(Settings.DocDBName),
                        new DocumentCollection { Id = Settings.DocDBCollection },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<HealthCheckOutput> HealthCheck()
        {
            var res = new HealthCheckOutput
            {
                CollectionError = "N/A"
            };

            try
            {
                await client.ReadDocumentCollectionAsync(baseDocCollectionUri);
                res.CollectionAvailable = true;
            }
            catch (Exception e)
            {
                res.CollectionAvailable = false;
                res.CollectionError = e.Message;
            }

            res.ConsistencyLevel = client.ConsistencyLevel.ToString();
            res.ServiceEndpoint = client.ServiceEndpoint.AbsoluteUri;
            res.ReadEndpoint = client.ReadEndpoint.AbsoluteUri;
            res.WriteEndpoint = client.WriteEndpoint.AbsoluteUri;

            return res;
        }

        public class HealthCheckOutput
        {
            public bool CollectionAvailable { get; set; }
            public string CollectionError { get; set; }
            public string ConsistencyLevel { get; set; }
            public string ServiceEndpoint { get; set; }
            public string ReadEndpoint { get; set; }
            public string WriteEndpoint { get; set; }

        }

    }
}
