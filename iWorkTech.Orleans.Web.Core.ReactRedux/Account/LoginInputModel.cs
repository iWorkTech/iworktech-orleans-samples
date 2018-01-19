// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace iWorkTech.Orleans.Web.Core.ReactRedux.Account
{
    [JsonObject]
    public class LoginInputModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool RememberLogin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReturnUrl { get; set; }
    }
}