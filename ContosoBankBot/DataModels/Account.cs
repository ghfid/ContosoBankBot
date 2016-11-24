using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot.DataModels
{
    public class Account
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "ac_name")]
        public string AccName { get; set; }

        [JsonProperty(PropertyName = "ac_num")]
        public string AccNum { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public double Balance { get; set; }

        [JsonProperty(PropertyName = "saving_ac")]
        public double SavingAcc { get; set; }

        [JsonProperty(PropertyName = "int_rate")]
        public double IntRate { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

    }
}