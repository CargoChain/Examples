using CargoChain.Sdk.CSharp.Messages.Profiles;
using eShop.Carrier.Models;
using eShop.Lib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace eShop.Carrier.Data
{
    public class CargoChainService : CargoChainServiceBase
    {
        private readonly CarrierContext _carrierContext;

        public CargoChainService(CarrierContext carrierContext, IOptionsMonitor<CargoChainConfiguration> optionsMonitor, ILogger<CargoChainServiceBase> logger)
            : base(optionsMonitor, logger) 
        {
            _carrierContext = carrierContext;
        }

        public async Task EnsureProfilesSubscription()
        {
            if (_carrierContext.Subscriptions.Count() == 0)
            {
                // Create profile subscription
                var response = await ApiClient.Profile.RegisterProfileHook(new RegisterProfileHookRequest
                {
                    EventTypes = new string[] { "ProductState" },
                    Uri = CargoChainConfiguration.WebHookUrl.ToString()
                });

                ValidateCargoChainApiResponse(response, nameof(EnsureProfilesSubscription));

                _carrierContext.Subscriptions.Insert(new CargoChainSubscription { SubscriptionId = response.Data.Id });
            }
        }
    }
}
