using System.Text.Json.Serialization;

namespace FTPClient.Models
{
    public class UserSettings
    {
        [JsonPropertyName("localPath")]
        public string LocalPath { get; set; }
    }
}
