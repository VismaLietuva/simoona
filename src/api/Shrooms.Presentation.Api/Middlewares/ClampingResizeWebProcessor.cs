using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>
    /// Drop-in replacement for the default <see cref="ResizeWebProcessor"/> that restores
    /// the ImageResizer.NET behaviour the legacy frontend was written against:
    /// 1. Claims the legacy <c>mode</c> query-string key so ImageSharp.Web's request parser
    ///    passes it through to <c>OnParseCommandsAsync</c> for aliasing to <c>rmode</c>.
    ///    Without this the alias is a no-op, <c>rmode</c> is never set, and the resize
    ///    falls through to its default mode (stretch to exact requested size).
    /// 2. Skips the resize entirely when the source already fits within the requested
    ///    bounds — equivalent to ImageResizer.NET's <c>scale=downscaleonly</c> default,
    ///    avoiding the upscaling that ImageSharp.Web does for sub-bound sources.
    /// The office floor-plan view stores room polygon coordinates in the served image's
    /// natural-pixel space (see <c>room-manage.service.js</c>); any silent rescale here
    /// shifts every polygon point and rooms drift away from their walls.
    /// </summary>
    public class ClampingResizeWebProcessor : IImageWebProcessor
    {
        private readonly ResizeWebProcessor _inner = new();
        private readonly string[] _commands;

        public ClampingResizeWebProcessor()
        {
            // ImageSharp.Web's request parser drops any query-string key that isn't
            // claimed by *some* registered processor's Commands list. The legacy frontend
            // sends 'mode=max' (ImageResizer.NET convention); without claiming 'mode'
            // here it never reaches OnParseCommandsAsync, the alias to 'rmode' never
            // fires, and the resize falls back to its default mode (stretch to exact
            // requested size) — which is exactly the floor-plan distortion this class
            // exists to prevent.
            _commands = _inner.Commands.Concat(new[] { "mode" }).ToArray();
        }

        public IEnumerable<string> Commands => _commands;

        public bool RequiresTrueColorPixelFormat(
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
            => _inner.RequiresTrueColorPixelFormat(commands, parser, culture);

        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture)
        {
            var reqW = TryParseDimension(commands, ResizeWebProcessor.Width);
            var reqH = TryParseDimension(commands, ResizeWebProcessor.Height);

            var srcW = image.Image.Width;
            var srcH = image.Image.Height;

            var needsResize =
                (reqW.HasValue && srcW > reqW.Value) ||
                (reqH.HasValue && srcH > reqH.Value);

            if (!needsResize)
            {
                return image;
            }

            return _inner.Process(image, logger, commands, parser, culture);
        }

        private static int? TryParseDimension(CommandCollection commands, string key)
        {
            if (!commands.TryGetValue(key, out var raw))
            {
                return null;
            }

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return null;
            }

            return value;
        }
    }
}
