using RazorLight;

namespace Shrooms.Infrastructure.Email.Templating;

/// <summary>
/// Creates a file-system-backed <see cref="IRazorLightEngine"/> rooted at the supplied base path.
/// </summary>
public static class RazorLightEngineFactory
{
    /// <summary>
    /// Builds and returns a <see cref="IRazorLightEngine"/> configured to resolve templates
    /// relative to <paramref name="emailTemplatesBasePath"/>.
    /// </summary>
    public static IRazorLightEngine Create(string emailTemplatesBasePath)
    {
        return new RazorLightEngineBuilder()
            .UseFileSystemProject(emailTemplatesBasePath, ".html")
            .UseMemoryCachingProvider()
            .Build();
    }
}
