namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookRemindService
    {
        void RemindAboutBooks(int daysBefore);
    }
}
