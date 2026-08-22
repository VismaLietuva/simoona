namespace Shrooms.Domain.Services.Wall.Mentions
{
    public interface IMentionLinkExpander
    {
        /// <summary>
        /// Rewrites the mention tokens in a message body into markdown links with
        /// absolute URLs, so a mention is clickable in an email rather than dead
        /// relative markup. Pass a null organization to get bold text instead, for
        /// contexts with no tenant in hand.
        /// </summary>
        string ExpandToMarkdown(string messageBody, string organizationShortName);
    }
}
