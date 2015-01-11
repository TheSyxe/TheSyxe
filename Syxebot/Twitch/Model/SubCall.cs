using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syxebot.Twitch.Model
{
    public class SubCall
    {
        [JsonProperty("_total")]
        public int Total;

        [JsonProperty("_links")]
        public Links Links;

        [JsonProperty("subscriptions")]
        public List<Subscription> Subs;
    }

    public class Links
    {
        [JsonProperty("next")]
        public string Next;

        [JsonProperty("self")]
        public string Self;
    }

    public class Subscription
    {
        [JsonProperty("_id")]
        public string ID;

        [JsonProperty("user")]
        public User User;

        [JsonProperty("created_at")]
        public string CreatedAt;

        [JsonProperty("_links")]
        public Links Links;
    }

    public class User
    {
        [JsonProperty("_id")]
        public int ID;

        [JsonProperty("logo")]
        public string Logo;

        [JsonProperty("staff")]
        public bool Staff;

        [JsonProperty("created_at")]
        public string CreatedAt;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("updated_at")]
        public string UpdatedAt;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("_links")]
        public Links Links;
    }
}
