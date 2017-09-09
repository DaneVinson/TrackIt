using Microsoft.Azure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.DocumentDB
{
    /// <summary>
    /// Static utility class for the TrackIt.Data.DocumentDB project.
    /// </summary>
    public static class DocumentDBUtility
    {
        #region Public Utilty

        /// <summary>
        /// Method to create the database collection associated with the specified model type using
        /// the current configuration settings.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CreateCollectionAsync<T>(IDocumentDBConfiguration configuration)
        {
            string collectionName = ModelUtility.GetPluralizedModelName<T>();
            using (var client = NewDocumentClient(configuration))
            {
                string collectionsFeed = GetCollectionsFeed(configuration);
                DocumentCollection collection = client.CreateDocumentCollectionQuery(collectionsFeed)
                                                        .Where(c => c.Id == collectionName)
                                                        .AsEnumerable()
                                                        .FirstOrDefault();
                if (collection == null)
                {
                    collection = await client.CreateDocumentCollectionAsync(
                                                collectionsFeed,
                                                new DocumentCollection { Id = collectionName });
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Method to create the database as specified by the configuration's document database name.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CreateDatabaseAsync(IDocumentDBConfiguration configuration)
        {
            using (var client = NewDocumentClient(configuration))
            {
                var database = client.CreateDatabaseQuery()
                                        .Where(d => configuration.DatabaseName.Equals(d.Id))
                                        .AsEnumerable()
                                        .FirstOrDefault();
                if (database == null)
                {
                    database = await client.CreateDatabaseAsync(new Database() { Id = configuration.DatabaseName });
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Internal Utilty

        /// <summary>
        /// Get a string for the collection feed based on configuration setting for the database name.
        /// </summary>
        /// <returns></returns>
        internal static string GetCollectionsFeed(IDocumentDBConfiguration configuration)
        {
            return $"dbs/{configuration.DatabaseName}/";
        }

        /// <summary>
        /// Get a string for a document feed based on configuration setting for the database name and the
        /// type of the model in the collection
        /// </summary>
        /// <returns></returns>
        internal static string GetDocumentFeed<T>(IDocumentDBConfiguration configuration) where T : IModel
        {
            return $"{GetCollectionsFeed(configuration)}colls/{ModelUtility.GetPluralizedModelName<T>()}";
        }

        // TODO: Refactor to use DocumentDB SDK's UriFactory to construct URI's
        internal static string GetDocumentLink<T>(string id, IDocumentDBConfiguration configuration) where T : IModel
        {
            return $"{GetDocumentFeed<T>(configuration)}/docs/{id}";
        }

        internal static DocumentClient NewDocumentClient(IDocumentDBConfiguration configuration)
        {
            Uri uri;
            if (!Uri.TryCreate(configuration.AccountUri, UriKind.Absolute, out uri))
            {
                uri = new Uri("http://invalid_uri");
            }
            return new DocumentClient(uri, configuration.AccountKey);
        }

        #endregion
    }
}
