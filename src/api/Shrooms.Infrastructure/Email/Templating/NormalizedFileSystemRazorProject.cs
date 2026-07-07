using RazorLight.Razor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Infrastructure.Email.Templating
{
    /// <summary>
    /// Extends FileSystemRazorProject to normalize layout keys that RazorLight produces
    /// when resolving relative Layout paths (e.g. "../HeaderFooter") from subdirectory
    /// templates. RazorLight prepends "/" to template keys internally, producing paths
    /// like "/../HeaderFooter.cshtml" which Path.IsPathRooted treats as rooted on Windows,
    /// causing the root directory prefix to be skipped and the file to not be found.
    /// </summary>
    public class NormalizedFileSystemRazorProject : FileSystemRazorProject
    {
        public NormalizedFileSystemRazorProject(string root) : base(root) { }

        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
            => base.GetItemAsync(NormalizeKey(templateKey));

        private static string NormalizeKey(string key)
        {
            // Strip any leading slashes that RazorLight prepends internally, then
            // collapse ".." segments so "/../HeaderFooter.cshtml" → "HeaderFooter.cshtml".
            var parts = key.TrimStart('/', '\\').Split('/', '\\');
            var segments = new List<string>();
            foreach (var part in parts)
            {
                if (part == "..")
                {
                    // Pop the last segment if possible; otherwise ignore (can't go above root)
                    if (segments.Count > 0)
                        segments.RemoveAt(segments.Count - 1);
                }
                else if (part != "." && part.Length > 0)
                {
                    segments.Add(part);
                }
            }
            return string.Join("/", segments);
        }
    }
}
