namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books
{
    public interface IBookRemindService
    {
        void RemindAboutBooks(int daysBefore);
    }
}
