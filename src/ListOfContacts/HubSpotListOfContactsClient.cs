using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Logging;
using RapidCore.Network;
using Skarp.HubSpotClient.Company.Dto;
using Skarp.HubSpotClient.Company.Interfaces;
using Skarp.HubSpotClient.Contact.Dto;
using Skarp.HubSpotClient.Core;
using Skarp.HubSpotClient.Core.Interfaces;
using Skarp.HubSpotClient.Core.Requests;
using Skarp.HubSpotClient.ListOfContacts.Dto;
using Skarp.HubSpotClient.ListOfContacts.Interfaces;

namespace Skarp.HubSpotClient.ListOfContacts
{
    public class HubSpotListOfContactsClient : HubSpotBaseClient
    {
        /// <summary>
        /// Mockable and container ready constructor
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="logger"></param>
        /// <param name="serializer"></param>
        /// <param name="hubSpotBaseUrl"></param>
        /// <param name="apiKey"></param>
        public HubSpotListOfContactsClient(
            IRapidHttpClient httpClient,
            ILogger<HubSpotListOfContactsClient> logger,
            RequestSerializer serializer,
            string hubSpotBaseUrl,
            string apiKey)
            : base(httpClient, logger, serializer, hubSpotBaseUrl, apiKey)
        {
        }

        /// <summary>
        /// Create a new instance of the HubSpotListOfContactsClient with default dependencies
        /// </summary>
        /// <remarks>
        /// This constructor creates a HubSpotListOfContactsClient using "real" dependencies that will send requests 
        /// via the network - if you wish to have support for functional tests and mocking use the "eager" constructor
        /// that takes in all underlying dependecies
        /// </remarks>
        /// <param name="apiKey">Your API key</param>
        public HubSpotListOfContactsClient(string apiKey)
        : base(
              new RealRapidHttpClient(new HttpClient()),
              NoopLoggerFactory.Get(),
              new RequestSerializer(new RequestDataConverter(NoopLoggerFactory.Get<RequestDataConverter>())),
              "https://api.hubapi.com",
              apiKey)
        { }

        /// <summary>
        /// Return a list of contacts for a contact list by id from hubspot
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetListByIdAsync<T>(long listId, ListOfContactsRequestOptions opts = null)
        {
            Logger.LogDebug("Get contacts for list with id");
            if (opts == null)
            {
                opts = new ListOfContactsRequestOptions();
            }
            var path = PathResolver(new ContactHubSpotEntity(), HubSpotAction.Get)
                .Replace(":listId:", listId.ToString())
                .SetQueryParam("count", opts.NumberOfContactsToReturn);
            if (opts.ContactOffset.HasValue)
            {
                path = path.SetQueryParam("vidOffset", opts.ContactOffset);
            }
            var data = await GetGenericAsync<T>(path);
            return data;
        }

        /// <summary>
        /// Add list of contacts based on list id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<bool> AddBatchAsync(HubSpotListOfContactsEntity contacts, long listId)
        {
            Logger.LogDebug("Add batch of contacts to list of contacts with specified id");

            var path = PathResolver(new ContactHubSpotEntity(), HubSpotAction.CreateBatch)
                .Replace(":listId:", listId.ToString());
            var data = await PutOrPostGeneric(path, contacts, true);
            return data;
        }

        /// <summary>
        /// Remove list of contacts based on list id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<bool> RemoveBatchAsync(HubSpotListOfContactsEntity contacts, long listId)
        {
            Logger.LogDebug("Remove batch of contacts to list of contacts with specified id");

            var path = PathResolver(new ContactHubSpotEntity(), HubSpotAction.DeleteBatch)
                .Replace(":listId:", listId.ToString());
            var data = await PutOrPostGeneric(path, contacts, true);
            return data;
        }

        /// <summary>
        /// Resolve a hubspot API path based off the entity and opreation that is about to happen
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string PathResolver(Contact.Interfaces.IContactHubSpotEntity entity, HubSpotAction action)
        {
            switch (action)
            {
                case HubSpotAction.Get:
                    return $"{entity.RouteBasePath}/lists/:listId:/contacts/all";
                case HubSpotAction.CreateBatch:
                    return $"{entity.RouteBasePath}/lists/:listId:/add";
                case HubSpotAction.DeleteBatch:
                    return $"{entity.RouteBasePath}/lists/:listId:/remove";
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}
