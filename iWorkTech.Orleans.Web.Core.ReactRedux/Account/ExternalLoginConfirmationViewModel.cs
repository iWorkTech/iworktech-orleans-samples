using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace iWorkTech.Orleans.Web.Core.ReactRedux.Account
{
    [JsonObject]
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
