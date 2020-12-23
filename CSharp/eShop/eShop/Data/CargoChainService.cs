using CargoChain.Sdk.CSharp;
using CargoChain.Sdk.CSharp.Messages;
using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Data
{
    public class CargoChainService : IDisposable
    {
        private class CargoChainAccessTokenData
        {
            public string AccessToken { get; set; }
            public string TokenType { get; set; }
            public int ExpiresIn { get; set; }
        }

        private readonly CargoChainConfiguration _cargoChainConfiguration;
        private readonly ILogger<CargoChainService> _logger;
        private CargoChainClient _apiClient;
        private static HttpClient _portalClient;

        public CargoChainService(IOptionsMonitor<CargoChainConfiguration> optionsMonitor, ILogger<CargoChainService> logger)
        {
            _cargoChainConfiguration = optionsMonitor.CurrentValue;
            _logger = logger;
            InitializePortalClient();
            InitializeApiClient();
        }

        public async Task<ProfileResponse> CreateProductProfile(Product product)
        {
            var profileResult = await _apiClient.Profile.CreateProfiles(new CreateProfilesRequest[]
            {
                new CreateProfilesRequest
                {
                    Alias = product.Id.ToString(),
                    ProfileType = "eShopProduct",
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

        public async Task<AddEventsResponse> UpdateProfileState(string profileSecretId, ProductState state)
        {
            var addEventResult = await _apiClient.Profile.AddEvents(new AddEventsRequest[]
            {
                new AddEventsRequest
                {
                    ProfileSecretId = profileSecretId,
                    Events = new EventRequest[]
                    {
                        GetProductStateEventRequest(state)
                    }
                }
            });

            ValidateCargoChainApiResponse(addEventResult, nameof(UpdateProfileState));

            return addEventResult.Data[0];
        }

        public async Task<AddEventsResponse> OrderProductProfile(Product product)
        {
            var addEventResult = await _apiClient.Profile.AddEvents(new AddEventsRequest[]
            {
                new AddEventsRequest
                {
                    ProfileSecretId = product.CargoChainProfileSecretId,
                    Events = new EventRequest[]
                    {
                        new EventRequest
                        {
                            EventType = "DeliveryAddress",
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
                EventType = "ProductInformation",
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

        private EventRequest GetProductStateEventRequest(ProductState state)
        {
            return new EventRequest
            {
                EventType = "ProductState",
                Visibility = EventVisibility.Public,
                Properties = new EventPropertyRequest[]
                {
                    new EventPropertyRequest
                    {
                        DataType = "text",
                        Value = state.ToString(),
                        Name = "State"
                    }
                }
            };
        }

        private void ValidateCargoChainApiResponse(IResponse response, string method)
        {
            if (!response.IsSuccess)
            {
                var message = $"Unexpected error in {method}. The CargoChain API call has failed: {response.Message}";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }
        }

        private void InitializePortalClient()
        {
            _portalClient = new HttpClient();
            _portalClient.BaseAddress = _cargoChainConfiguration.PortalUrl;
        }

        private void InitializeApiClient()
        {
            _apiClient = new CargoChainClient(_cargoChainConfiguration.ApiUrl);
            SetApiClientAccessToken();
            _apiClient.AccessTokenExpired += (s, e) =>
            {
                SetApiClientAccessToken();
            };
        }

        private void SetApiClientAccessToken()
        {
            var valueToEncode = Encoding.UTF8.GetBytes($"{_cargoChainConfiguration.ClientId}:{_cargoChainConfiguration.ClientSecret}");
            var encodedValue = Convert.ToBase64String(valueToEncode);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/runAs"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedValue);
                requestMessage.Content = new StringContent(_cargoChainConfiguration.RunAsKey);
                try
                {
                    var response = _portalClient.SendAsync(requestMessage).Result.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<CargoChainAccessTokenData>(response);
                    _apiClient.AccessToken = token.AccessToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error when trying to get a new CargoChain access token");
                }
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
            _portalClient?.Dispose();
        }
    }
}
