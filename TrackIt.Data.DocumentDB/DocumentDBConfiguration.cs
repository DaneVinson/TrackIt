using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Data.DocumentDB
{
    /// <summary>
    /// Configiuration object for DocumentDB. Loads read-only properties from configuration appSettings
    /// AzureDocumentDBAccountKey, AzureDocumentDBAccountUri and AzureDocumentDBName.
    /// </summary>
    public class DocumentDBConfiguration : IDocumentDBConfiguration
    {
        public DocumentDBConfiguration(string accountKey, string accountUri, string databaseName)
        {
            AccountKey = accountKey;
            AccountUri = accountUri;
            DatabaseName = databaseName;
        }


        /// <summary>
        /// The Azure DocumentDB account key as found in the appSetting AzureDocumentDBAccountKey.
        /// </summary>
        public string AccountKey { get; }

        /// <summary>
        /// The Azure DocumentDB account URI as found in the appSetting AzureDocumentDBAccountUri.
        /// </summary>
        public string AccountUri { get; }

        /// <summary>
        /// The Azure DocumentDB name as found in the appSetting AzureDocumentDBName.
        /// </summary>
        public string DatabaseName { get; }
    }
}
