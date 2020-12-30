using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CargoChain.Sdk.CSharp;
using CargoChain.Sdk.CSharp.Messages;
using CargoChain.Sdk.CSharp.Messages.Profiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace eShop.Lib
{
    public abstract class CargoChainServiceBase : IDisposable
    {
        private class CargoChainAccessTokenData
        {
            public string AccessToken { get; set; }
            public string TokenType { get; set; }
            public int ExpiresIn { get; set; }
        }

        private static HttpClient _portalClient;
        
        protected CargoChainConfiguration CargoChainConfiguration { get; }
        protected ILogger<CargoChainServiceBase> Logger { get; }
        protected CargoChainClient ApiClient { get; private set; }

        public CargoChainServiceBase(CargoChainConfiguration cargoChainConfiguration, ILogger<CargoChainServiceBase> logger)
        {
            CargoChainConfiguration = cargoChainConfiguration;
            Logger = logger;
            InitializePortalClient();
            InitializeApiClient();
        }

        public async Task<AddEventsResponse> UpdateProfileState(string profileSecretId, ProductState state)
        {
            var addEventResult = await ApiClient.Profile.AddEvents(new AddEventsRequest[]
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

        public string GetPropertyValue(EventResponse evt, string propertyName)
        {
            return JsonConvert.DeserializeObject<string>(evt.EventBody.Properties.First(x => x.Name.Equals(propertyName)).JsonValue);
        }

        protected void ValidateCargoChainApiResponse(IResponse response, string method)
        {
            if (!response.IsSuccess)
            {
                var message = $"Unexpected error in {method}. The CargoChain API call has failed: {response.Message}";
                Logger.LogError(message);
                throw new ApplicationException(message);
            }
        }

        protected EventRequest GetProductStateEventRequest(ProductState state)
        {
            return new EventRequest
            {
                EventType = ProductEventTypes.ProductState,
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

        private void InitializePortalClient()
        {
            _portalClient = new HttpClient();
            _portalClient.BaseAddress = CargoChainConfiguration.PortalUrl;
        }

        private void InitializeApiClient()
        {
            ApiClient = new CargoChainClient(CargoChainConfiguration.ApiUrl);
            SetApiClientAccessToken();
            ApiClient.AccessTokenExpired += (s, e) =>
            {
                SetApiClientAccessToken();
            };
        }

        private void SetApiClientAccessToken()
        {
            var valueToEncode = Encoding.UTF8.GetBytes($"{CargoChainConfiguration.ClientId}:{CargoChainConfiguration.ClientSecret}");
            var encodedValue = Convert.ToBase64String(valueToEncode);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/runAs"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedValue);
                requestMessage.Content = new StringContent(CargoChainConfiguration.RunAsKey);
                try
                {
                    var response = _portalClient.SendAsync(requestMessage).Result.Content.ReadAsStringAsync().Result;
                    var token = JsonConvert.DeserializeObject<CargoChainAccessTokenData>(response);
                    ApiClient.AccessToken = token.AccessToken;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error when trying to get a new CargoChain access token");
                }
            }
        }

        public void Dispose()
        {
            ApiClient?.Dispose();
            _portalClient?.Dispose();
        }
    }
}
