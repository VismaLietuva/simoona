using System.Globalization;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>
    /// Caps the width/height commands of an anonymous resize request. ImageSharp.Web allocates the full
    /// requested canvas, so an unbounded "height=100000" would cost hundreds of megabytes per request
    /// and each distinct dimension pair becomes a new file in the on-disk cache.
    /// </summary>
    public static class ResizeCommandGuard
    {
        public const int MaxDimension = 2048;

        public static void Clamp(CommandCollection commands, int maxDimension = MaxDimension)
        {
            ClampCommand(commands, ResizeWebProcessor.Width, maxDimension);
            ClampCommand(commands, ResizeWebProcessor.Height, maxDimension);
        }

        private static void ClampCommand(CommandCollection commands, string key, int maxDimension)
        {
            if (!commands.TryGetValue(key, out var raw))
            {
                return;
            }

            // Non-numeric or non-positive values are dropped so the processor falls back to its default
            // instead of interpreting garbage.
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value <= 0)
            {
                commands.Remove(key);
                return;
            }

            if (value > maxDimension)
            {
                commands.Remove(key);
                commands.Add(key, maxDimension.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
