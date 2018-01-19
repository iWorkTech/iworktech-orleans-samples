using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Webapp.Models;

namespace iWorkTech.Orleans.Web.Core.ReactRedux.Account
{
    [JsonObject]
    public class RegisterResponseModel: ApiModel
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string NewXsrfToken { get; set; }
    }
}
