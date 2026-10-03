using System.Globalization;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>
    /// Normalises the width/height commands of an anonymous resize request. Every distinct dimension pair is
    /// a decode, a resize and a file in the on-disk cache, so requests are snapped up to a small set of
    /// sizes: a browser asking for 100px gets the 128px variant, and an attacker cannot mint millions of
    /// variants per image. Oversized or malformed values are capped or dropped.
    /// </summary>
    public static class ResizeCommandGuard
    {
        public const int MaxDimension = 2048;

        public static readonly int[] AllowedSizes = { 32, 64, 96, 128, 192, 256, 384, 480, 640, 768, 1024, 1280, 1600, MaxDimension };

        public static void Clamp(CommandCollection commands)
        {
            NormaliseCommand(commands, ResizeWebProcessor.Width);
            NormaliseCommand(commands, ResizeWebProcessor.Height);
        }

        public static int Snap(int requested)
        {
            foreach (var size in AllowedSizes)
            {
                if (requested <= size)
                {
                    return size;
                }
            }

            return MaxDimension;
        }

        private static void NormaliseCommand(CommandCollection commands, string key)
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

            var snapped = Snap(value);
            if (snapped != value)
            {
                commands.Remove(key);
                commands.Add(key, snapped.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
