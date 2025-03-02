using System.Threading.Tasks;

namespace TheGreenBowl.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string from, string subject, string body);
    }
}