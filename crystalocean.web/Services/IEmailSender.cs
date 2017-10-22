using System;
using System.Threading.Tasks;

namespace CrystalOcean.Web.Services
{
    public interface IEmailSender
    {
         Task SendEmailAsync(String email, String subject, String message);
    }
}