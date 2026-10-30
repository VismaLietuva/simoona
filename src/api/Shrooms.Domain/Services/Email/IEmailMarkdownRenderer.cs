namespace Shrooms.Domain.Services.Email
{
    // Renders a user-authored markdown body for an email of one organisation: mention tokens
    // become profile links, the organisation's custom emoji become images served by the API, and
    // standard shortcodes become Unicode. Empty input renders to an empty string.
    public interface IEmailMarkdownRenderer
    {
        Task<string> RenderAsync(string markdown, int organizationId, string organizationShortName);
    }
}
