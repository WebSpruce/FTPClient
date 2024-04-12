using Avalonia.Media;
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
        [JsonPropertyName("profileColor")]
        public JsonColor ProfileColor { get; set; }
    }
    public class JsonColor
    {
        [JsonPropertyName("A")]
        public byte A { get; set; }
        [JsonPropertyName("R")]
        public byte R { get; set; }
        [JsonPropertyName("G")]
        public byte G { get; set; }
        [JsonPropertyName("B")]
        public byte B { get; set; }

        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

}
