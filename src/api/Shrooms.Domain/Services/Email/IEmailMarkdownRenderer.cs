namespace Shrooms.Domain.Services.Email
{
    public interface IEmailMarkdownRenderer
    {
        Task<string> RenderAsync(string markdown, int organizationId, string organizationShortName, int? maxTextLength = null);
    }
}
