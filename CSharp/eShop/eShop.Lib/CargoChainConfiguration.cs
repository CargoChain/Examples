﻿using System;

namespace eShop.Lib
{
    public class CargoChainConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RunAsKey { get; set; }
        public Uri PortalUrl { get; set; }
        public Uri ApiUrl { get; set; }
        public Uri PublicViewUrl { get; set; }
        public Uri WebHookUrl { get; set; }
    }
}
