using System;
using System.Globalization;
using System.Linq;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>Canonical, bounded resize commands: the disk cache is keyed on command values and every variant is a decode and a file.</summary>
    public static class ResizeCommandGuard
    {
        public const int MaxDimension = 2048;

        public static readonly int[] AllowedSizes = { 32, 64, 96, 128, 192, 256, 384, 480, 640, 768, 1024, 1280, 1600, MaxDimension };

        // Modes whose output never exceeds the requested box ("min" does not). Clients use max and crop.
        public static readonly string[] AllowedModes = { "max", "crop", "pad", "stretch", "boxpad" };

        private static readonly string[] KeptCommands = { ResizeWebProcessor.Width, ResizeWebProcessor.Height, ResizeWebProcessor.Mode };

        public static void Clamp(CommandCollection commands)
        {
            NormaliseDimension(commands, ResizeWebProcessor.Width);
            NormaliseDimension(commands, ResizeWebProcessor.Height);

            if (!commands.Contains(ResizeWebProcessor.Width) && !commands.Contains(ResizeWebProcessor.Height))
            {
                commands.Clear();
                return;
            }

            NormaliseMode(commands);

            // Every other command multiplies cache variants, and some values make the processor throw.
            foreach (var key in commands.Select(c => c.Key).Where(k => !KeptCommands.Contains(k, StringComparer.OrdinalIgnoreCase)).ToArray())
            {
                commands.Remove(key);
            }
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

        private static void NormaliseDimension(CommandCollection commands, string key)
        {
            if (!commands.TryGetValue(key, out var raw))
            {
                return;
            }

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value <= 0)
            {
                commands.Remove(key);
                return;
            }

            // Canonical spelling: "0128", "+128" and "128" must share one cache entry.
            Replace(commands, key, raw, Snap(value).ToString(CultureInfo.InvariantCulture));
        }

        private static void NormaliseMode(CommandCollection commands)
        {
            if (!commands.TryGetValue(ResizeWebProcessor.Mode, out var raw))
            {
                return;
            }

            var canonical = AllowedModes.FirstOrDefault(m => string.Equals(m, raw?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (canonical == null)
            {
                commands.Remove(ResizeWebProcessor.Mode);
                return;
            }

            Replace(commands, ResizeWebProcessor.Mode, raw, canonical);
        }

        private static void Replace(CommandCollection commands, string key, string raw, string canonical)
        {
            if (!string.Equals(raw, canonical, StringComparison.Ordinal))
            {
                commands.Remove(key);
                commands.Add(key, canonical);
            }
        }
    }
}
