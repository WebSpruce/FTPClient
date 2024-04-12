using Avalonia.Media;

namespace FTPClient.Helper
{
    internal static class DarkOrLightColor
    {
        public static bool IsLightColor(Color color)
        {
            int luminance = (int)((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B));
            return luminance > 127;
        }
    }
}
