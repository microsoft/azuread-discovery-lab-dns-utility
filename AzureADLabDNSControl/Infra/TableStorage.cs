using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureADLabDNSControl.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Infra
{
    public class TableStorage
    {
        public static string StorageConnectionString { get; set; }

        public static async Task<IEnumerable<LabDTO>> AddNewLab(LabSettings lab, string[] domains)
        {
            return await AddNewLab(domains, lab.City, lab.PartitionKey, lab.LabDate);
        }

        public static async Task<IEnumerable<LabDTO>> AddNewLab(string[] domains, string city, string instructor, DateTime labDate)
        {
            try
            {
                CloudTable table = await CreateTableAsync("labsettings");
                LabSettings item = new LabSettings(instructor, labDate, city);

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);

                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

                LabSettings newLab = result.Result as LabSettings;
                string auth = null;
                foreach(string dom in domains)
                {
                    auth = string.Format("Team-{0}", Util.CreatePassword(5));
                    await SetDomAssignmentAsync(newLab.LabCode, dom, auth, null);
                }

                return await GetLabs(instructor);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<LabDTO> GetLab(string instructor, DateTime labDate)
        {
            var res = new LabDTO();
            CloudTable table = await CreateTableAsync("labsettings");
            string dte = JsonConvert.SerializeObject(labDate);
            TableOperation retrieveOperation = TableOperation.Retrieve<LabSettings>(instructor, dte);

            TableResult result = await table.ExecuteAsync(retrieveOperation);
            res.Lab = result.Result as LabSettings;
            res.Assignments = await GetDomAssignments(res.Lab.LabCode);
            return res;
        }


        public static async Task<IEnumerable<LabDTO>> GetLabs(string instructor)
        {
            var res = new List<LabDTO>();
            CloudTable table = await CreateTableAsync("labsettings");

            TableQuery<LabSettings> partitionScanQuery = new TableQuery<LabSettings>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, instructor));

            TableContinuationToken token = null;

            // Page through the results
            do
            {
                TableQuerySegment<LabSettings> segment = await table.ExecuteQuerySegmentedAsync(partitionScanQuery, token);
                token = segment.ContinuationToken;
                foreach (LabSettings entity in segment)
                {
                    res.Add(new LabDTO
                    {
                        Lab = entity,
                        Assignments = await GetDomAssignments(entity.LabCode)
                    });
                }
            }

            while (token != null);
            return res.ToList();
        }


        public static async Task<DomAssignment> GetDomAssignment(string labCode, string teamAuth)
        {
            CloudTable table = await CreateTableAsync("domassignment");
            TableOperation retrieveOperation = TableOperation.Retrieve<DomAssignment>(labCode, teamAuth);

            TableResult result = await table.ExecuteAsync(retrieveOperation);
            return result.Result as DomAssignment;
        }

        public static async Task<IEnumerable<DomAssignment>> GetDomAssignments(string labCode)
        {
            var res = new List<DomAssignment>();
            CloudTable table = await CreateTableAsync("domassignment");

            TableQuery<DomAssignment> partitionScanQuery = new TableQuery<DomAssignment>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, labCode));

            TableContinuationToken token = null;

            // Page through the results
            do
            {
                TableQuerySegment<DomAssignment> segment = await table.ExecuteQuerySegmentedAsync(partitionScanQuery, token);
                token = segment.ContinuationToken;
                foreach (DomAssignment entity in segment)
                {
                    res.Add(entity);
                }
            }

            while (token != null);
            return res.ToList();
        }

        public static async Task<DomAssignment> SetDomAssignmentAsync(string labCode, string domain, string TeamAuth, string txtRecord)
        {
            try
            {
                CloudTable table = await CreateTableAsync("domassignment");
                DomAssignment item = new DomAssignment(labCode, TeamAuth, domain);
                if (txtRecord != null)
                {
                    item.DnsTxtRecord = txtRecord;
                }

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);

                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

                DomAssignment newAssignment = result.Result as DomAssignment;
                return newAssignment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            return storageAccount;
        }

        private static async Task<CloudTable> CreateTableAsync(string tableName)
        {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(StorageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }

   
}