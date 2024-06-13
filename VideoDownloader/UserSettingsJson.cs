using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoDownloader
{
    [JsonObject("UserSettingsJson")]
    internal class UserSettingsJson
    {
        [JsonProperty("DownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("SaveDirectory")]
        public string SaveDirectory {  get; set; }
    }
}
