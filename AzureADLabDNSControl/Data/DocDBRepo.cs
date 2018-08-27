using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using AzureADLabDNSControl.Models;

namespace DocDBLib
{
    public static class DocDBRepo
    {
        private static DocumentClient client;
        private static Uri baseDocCollectionUri;

        public static class Settings
        {
            public static string DocDBUri;
            public static string DocDBAuthKey;
            public static string DocDBName;
            public static string DocDBCollection;
        }

        public static class DB<T> where T : class, IDocModelBase
        {
            public static async Task<T> CreateItemAsync(T item)
            {
                item.DocType = typeof(T).Name;
                item.Id = Guid.NewGuid().ToString();
                var res = await client.CreateDocumentAsync(baseDocCollectionUri, item);
                return item;
            }

            public static async Task<T> UpdateItemAsync(T item)
            {
                item.DocType = typeof(T).Name;
                var id = item.Id;
                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Settings.DocDBName, Settings.DocDBCollection, id), item);
                return item;
            }

            public static async Task<dynamic> DeleteItemAsync(T item)
            {
                var id = item.Id;
                return await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Settings.DocDBName, Settings.DocDBCollection, id));
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

            private static async Task<IEnumerable<T>> _getItemsAsync(IDocumentQuery<T> query)
            {
                List<T> results = new List<T>();
                try
                {
                    while (query.HasMoreResults)
                    {
                        results.AddRange(await query.ExecuteNextAsync<T>());
                    }

                    return results;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns all documents of type T where (predicate)
            /// </summary>
            /// <param name="predicate"></param>
            /// <returns></returns>
            public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
            {
                if (predicate == null) return await GetItemsAsync();

                var docType = typeof(T).Name;

                var cli = client.CreateDocumentQuery<T>(baseDocCollectionUri);
                var cli2 = cli.Where(predicate);

                Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);
                docTypeFilter.Compile();

                var query = cli2.Where(docTypeFilter)
                    .AsDocumentQuery();
                return await _getItemsAsync(query);
            }

            /// <summary>
            /// returns all documents of type T
            /// </summary>
            /// <returns></returns>
            public static async Task<IEnumerable<T>> GetItemsAsync()
            {
                var docType = typeof(T).Name;
                Expression<Func<T, bool>> docTypeFilter = (q => q.DocType == docType);

                var query = client.CreateDocumentQuery<T>(baseDocCollectionUri)
                    .Where(docTypeFilter)
                    .AsDocumentQuery();
                return await _getItemsAsync(query);
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
                    throw;
                }
            }

            public static async Task<string> UpdateTeamParms(string labId, string teamAuth, string field, string value, int retry=0)
            {
                try
                {
                    var res = await client.ExecuteStoredProcedureAsync<string>(UriFactory.CreateStoredProcedureUri(Settings.DocDBName, Settings.DocDBCollection, "UpdateAssignment"), labId, teamAuth, field, value);
                    return res;
                }
                catch (DocumentClientException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        if (retry >= 3)
                        {
                            throw new Exception("Multiple updates failed, please check the logs");
                        }

                        //update failed transactionally - retry
                        retry++;
                        return await UpdateTeamParms(labId, teamAuth, field, value, retry);
                    }
                    throw ex;
                }
                catch(Exception ex)
                {
                    //AddSPUpdateAssignment();
                    throw ex;
                }
            }

            public static async Task AddSPUpdateAssignment()
            {
                var updateAssignment = new StoredProcedure
                {
                    Id = "UpdateAssignment",
                    Body = GetSPText()
                };

                StoredProcedure createdStoredProcedure = await client.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection), updateAssignment);
            }

            private static string GetSPText()
            {
                return @"function updateAssignment(labId, teamAuth, field, value) {
    var context = getContext();  
    var coll = context.getCollection();  
    var link = coll.getSelfLink();  
    var response = context.getResponse();   
    if (!labId) throw new Error('LabId is undefined.');  
    if (!teamAuth) throw new Error('teamAuth is undefined.');
    if (!field) throw new Error('field is undefined')   
    if (!value) throw new Error('field is undefined')   

    var query = 'SELECT * FROM LabItems labs WHERE labs.id = ""' + labId + '""';   
    var run = coll.queryDocuments(link, query, { }, callback);

                function callback(err, docs)
                {
                    if (err) throw err;
                    if (docs.length > 0)
                    {
                        var team = null;
                        var arr = docs[0].domAssignments;
                        for (var teamIndex = 0; teamIndex < arr.length; teamIndex++)
                        {
                            if (arr[teamIndex].teamAuth == teamAuth)
                            {
                                team = arr[teamIndex];
                                break;
                            }
                        }
                        if (team == null)
                        {
                            throw new Error('The dom assignment was not found.')
                        }
                        UpdateDoc(docs[0], teamIndex);
                    }
                    else response.setBody('The document was not found.');
                }

                if (!run)
                {
                    throw new Error('The stored procedure could not be processed.');
                }

                function UpdateDoc(doc, index, team)
                {
                    switch (field)
                    {
                        case 'sessionId':
                            doc.domAssignments[index].sessionId = value;
                            break;
                        case 'dnsTxtRecord':
                            doc.domAssignments[index].dnsTxtRecord = value;
                            break;
                    }

                    var replace = coll.replaceDocument(doc._self, doc, { }, function(err, newdoc) {
                        if (err) throw err;
                        response.setBody(newdoc);
                    });

                if (!replace)
                {
                    throw new Error('The document could not be updated.');
                }
            }
        }";
            }
        }

        public static async Task<DocumentClient> Initialize()
        {
            baseDocCollectionUri = UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection);
            client = new DocumentClient(new Uri(Settings.DocDBUri), Settings.DocDBAuthKey);
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
    }
}
