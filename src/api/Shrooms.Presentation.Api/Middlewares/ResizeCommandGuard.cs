using System;
using System.Globalization;
using System.Linq;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>
    /// Reduces an anonymous resize request to a small, canonical command set. The on-disk cache is keyed on
    /// the command values, and every distinct combination is a decode, a resize and a file, so:
    /// width/height are snapped up to a fixed list of sizes, the resize mode is restricted to known values
    /// in canonical casing, every other command is dropped, and a request without any usable dimension is
    /// emptied so the middleware does not process (or cache) it at all and the original is served instead.
    /// </summary>
    public static class ResizeCommandGuard
    {
        public const int MaxDimension = 2048;

        public static readonly int[] AllowedSizes = { 32, 64, 96, 128, 192, 256, 384, 480, 640, 768, 1024, 1280, 1600, MaxDimension };

        // ImageSharp.Web's ResizeMode names. The clients use "max" and "crop".
        public static readonly string[] AllowedModes = { "max", "crop", "pad", "stretch", "min", "boxpad" };

        private static readonly string[] KeptCommands = { ResizeWebProcessor.Width, ResizeWebProcessor.Height, ResizeWebProcessor.Mode };

        public static void Clamp(CommandCollection commands)
        {
            NormaliseDimension(commands, ResizeWebProcessor.Width);
            NormaliseDimension(commands, ResizeWebProcessor.Height);

            // Without a dimension there is nothing to resize; letting the request through would re-encode
            // and cache a full-size copy per distinct spelling of the remaining commands.
            if (!commands.Contains(ResizeWebProcessor.Width) && !commands.Contains(ResizeWebProcessor.Height))
            {
                commands.Clear();
                return;
            }

            NormaliseMode(commands);

            // Sampler, anchor, centre coordinates, compand, orientation, the legacy "mode" alias once it has
            // been copied to rmode: each would multiply the cacheable variants per image without limit, and
            // some values make the processor throw.
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

            // Non-numeric or non-positive values are dropped so the processor falls back to its default
            // instead of interpreting garbage.
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value <= 0)
            {
                commands.Remove(key);
                return;
            }

            // Always rewrite to the canonical form: the cache is keyed on the command values, so "0128",
            // "+128" and "128" must collapse to one entry.
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
