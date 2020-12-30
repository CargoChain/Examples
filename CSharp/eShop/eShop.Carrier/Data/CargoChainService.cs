using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Carrier.Models;
using eShop.Lib;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eShop.Carrier.Data
{
    public class CargoChainService : CargoChainServiceBase
    {
        private readonly CarrierContext _carrierContext;

        public CargoChainService(CarrierContext carrierContext, CargoChainConfiguration cargoChainConfiguration, ILogger<CargoChainServiceBase> logger)
            : base(cargoChainConfiguration, logger) 
        {
            _carrierContext = carrierContext;
        }

        public async Task<ProfileResponse> GetProfileByPublicId(string profilePublicId)
        {
            var response = await ApiClient.Profile.GetProfileByPublicId(new GetProfileByPublicIdRequest
            {
                ProfilePublicId = profilePublicId
            });

            ValidateCargoChainApiResponse(response, nameof(GetProfileByPublicId));

            return response.Data;
        }

        public async Task<EventResponse[]> GetEvents(string profileSecretId, string lastEvent)
        {
            var response = await ApiClient.Profile.GetEvents(new GetEventsRequest
            {
                Count = 100,
                LastEvent = lastEvent,
                ProfileSecretId = profileSecretId
            });

            ValidateCargoChainApiResponse(response, nameof(GetEvents));

            return response.Data;
        }

        public async Task EnsureProfilesSubscription()
        {
            if (_carrierContext.Subscriptions.Count() == 0)
            {
                // Create profile subscription
                var response = await ApiClient.Profile.RegisterProfileHook(new RegisterProfileHookRequest
                {
                    EventTypes = new string[] { ProductEventTypes.ProductState },
                    Uri = CargoChainConfiguration.WebHookUrl.ToString()
                });

                ValidateCargoChainApiResponse(response, nameof(EnsureProfilesSubscription));

                _carrierContext.Subscriptions.Insert(new CargoChainSubscription { SubscriptionId = response.Data.Id });
            }
        }
    }
}
