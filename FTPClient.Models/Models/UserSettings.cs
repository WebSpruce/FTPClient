using System.Text.Json.Serialization;

namespace FTPClient.Models
{
    public class Profile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("userSettings")]
        public ProfileSettings ProfileSettings { get; set; }
    }
    public class ProfileSettings
    {
        [JsonPropertyName("localPath")]
        public string LocalPath { get; set; }
    }
}
