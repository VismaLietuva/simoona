namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailTemplateCache
    {
        void Add<T>(ICompiledEmailTemplate compiledEmailTemplate) where T : class;

        ICompiledEmailTemplate Get<T>() where T : class;
    }
}
