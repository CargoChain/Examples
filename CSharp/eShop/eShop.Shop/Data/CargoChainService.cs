using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Lib;
using eShop.Shop.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eShop.Shop.Data
{
    public class CargoChainService : CargoChainServiceBase
    {
        private readonly ShopContext _context;

        public CargoChainService(CargoChainConfiguration cargoChainConfiguration, ILogger<CargoChainService> logger, ShopContext context) 
            : base(cargoChainConfiguration, logger) 
        {
            _context = context;
        }

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

        public async Task EnsureProfilesSubscription()
        {
            if (_context.Subscriptions.Count() == 0)
            {
                // Create profile subscription
                var response = await ApiClient.Profile.RegisterProfileHook(new RegisterProfileHookRequest
                {
                    EventTypes = new string[] { ProductEventTypes.ProductState },
                    Uri = CargoChainConfiguration.WebHookUrl.ToString()
                });

                ValidateCargoChainApiResponse(response, nameof(EnsureProfilesSubscription));

                _context.Subscriptions.Insert(new CargoChainSubscription { SubscriptionId = response.Data.Id });
            }
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
                        Value = product.Description ?? "",
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
