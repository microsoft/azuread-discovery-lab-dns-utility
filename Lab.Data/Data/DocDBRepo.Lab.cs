using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using System.Net;

namespace DocDBLib
{
    public static partial class DocDBRepo
    {
        public static partial class DB<T> where T : class, IDocModelBase
        {
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
                    try
                    {
                        await AddSPUpdateAssignment();
                        retry++;
                        return await UpdateTeamParms(labId, teamAuth, field, value, retry);
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception("Error trying to install new Cosmos stored procedure.", ex2);
                    }
                }
            }

            public static async Task AddSPUpdateAssignment()
            {
                var updateAssignment = new StoredProcedure
                {
                    Id = "UpdateAssignment",
                    Body = GetSPText("updateAssignment")
                };

                StoredProcedure createdStoredProcedure = await client.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(Settings.DocDBName, Settings.DocDBCollection), updateAssignment);
            }
        }
    }
}
