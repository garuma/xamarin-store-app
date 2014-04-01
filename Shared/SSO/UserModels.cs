using System;
using System.Text;
using Newtonsoft.Json;

namespace Xamarin.SSO.Client
{
    [JsonObject (MemberSerialization.OptIn)]
    public class User
    {
        [JsonProperty (PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty (PropertyName = "firstname")]
        public string FirstName { get; set; }

        [JsonProperty (PropertyName = "lastname")]
        public string LastName { get; set; }
    }

    [JsonObject (MemberSerialization.OptIn)]
    public class AccountResponse
    {
        [JsonProperty (PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty (PropertyName = "error")]
        public string Error { get; set; }
        [JsonProperty (PropertyName = "user")]
        public User User { get; set; }
        [JsonProperty (PropertyName = "token")]
        public string Token { get; set; }
        // Expiration info?
    }

    [Flags]
    public enum Access
    {
        None = 0,
        Admin = 1,
        Write = 1 << 1,
        Read = 1 << 2,
    }
}
