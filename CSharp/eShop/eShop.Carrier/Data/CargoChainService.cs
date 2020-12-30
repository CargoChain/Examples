using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Carrier.Models;
using eShop.Lib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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

        public async Task<AddEventsResponse> AddProductPosition(string profileSecretId, ProductPosition position)
        {
            var addEventResult = await ApiClient.Profile.AddEvents(new AddEventsRequest[]
            {
                new AddEventsRequest
                {
                    ProfileSecretId = profileSecretId,
                    Events = new EventRequest[]
                    {
                        new EventRequest
                        {
                            EventType = ProductEventTypes.ProductPosition,
                            Visibility = EventVisibility.Public,
                            Properties = new EventPropertyRequest[]
                            {
                                new EventPropertyRequest
                                {
                                    DataType = "address",
                                    Value = position.Position,
                                    Name = "Position"
                                },
                                new EventPropertyRequest
                                {
                                    DataType = "temperature",
                                    Value = position.Temperature + " °C",
                                    Name = "Temperature"
                                },
                                new EventPropertyRequest
                                {
                                    DataType = "datetime",
                                    Value = position.PositionAt.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                    Name = "PositionAt"
                                },
                                new EventPropertyRequest
                                {
                                    DataType = "bool",
                                    Value = position.ProductDelivered.ToString(),
                                    Name = "ProductDelivered"
                                }
                            }
                        }
                    }
                }
            });

            ValidateCargoChainApiResponse(addEventResult, nameof(AddProductPosition));

            return addEventResult.Data[0];
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
