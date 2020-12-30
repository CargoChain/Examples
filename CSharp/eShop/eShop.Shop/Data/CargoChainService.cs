using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Lib;
using eShop.Shop.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eShop.Shop.Data
{
    public class CargoChainService : CargoChainServiceBase
    {
        public CargoChainService(CargoChainConfiguration cargoChainConfiguration, ILogger<CargoChainService> logger) 
            : base(cargoChainConfiguration, logger) { }

        public async Task<ProfileResponse> CreateProductProfile(Product product)
        {
            var profileResult = await ApiClient.Profile.CreateProfiles(new CreateProfilesRequest[]
            {
                new CreateProfilesRequest
                {
                    Alias = product.Id.ToString(),
                    ProfileType = ProfileTypes.Product,
                    Events = new EventRequest[]
                    {
                        GetProductInformationEventRequest(product),
                        GetProductStateEventRequest(product.State)
                    }
                }
            });

            ValidateCargoChainApiResponse(profileResult, nameof(CreateProductProfile));

            return profileResult.Data[0];
        }

        public async Task<AddEventsResponse> OrderProductProfile(Product product)
        {
            var addEventResult = await ApiClient.Profile.AddEvents(new AddEventsRequest[]
            {
                new AddEventsRequest
                {
                    ProfileSecretId = product.CargoChainProfileSecretId,
                    Events = new EventRequest[]
                    {
                        new EventRequest
                        {
                            EventType = ProductEventTypes.DeliveryAddress,
                            Visibility = EventVisibility.Public,
                            Properties = new EventPropertyRequest[]
                            {
                                new EventPropertyRequest
                                {
                                    DataType = "address",
                                    Value = product.DeliveryAddress,
                                    Name = "DeliveryAddress"
                                }
                            }
                        },
                        GetProductStateEventRequest(product.State)
                    }
                }
            });

            ValidateCargoChainApiResponse(addEventResult, nameof(OrderProductProfile));

            return addEventResult.Data[0];
        }

        private EventRequest GetProductInformationEventRequest(Product product)
        {
            return new EventRequest
            {
                EventType = ProductEventTypes.ProductInformation,
                Visibility = EventVisibility.Public,
                Properties = new EventPropertyRequest[]
                {
                    new EventPropertyRequest
                    {
                        DataType = "text",
                        Value = product.Name,
                        Name = "Name"
                    },
                    new EventPropertyRequest
                    {
                        DataType = "text",  
                        Value = product.Description,
                        Name = "Description"
                    },
                    new EventPropertyRequest
                    {
                        DataType = "decimal",
                        Value = product.Price.ToString(),
                        Name = "Price"
                    }
                }
            };
        }
    }
}
