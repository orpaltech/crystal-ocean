using System.Threading.Tasks;
using CrystalOcean.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CrystalOcean.Web.Controllers
{
    public class FileManagerController : Controller
    {
        private ImageWebApiSettings webApiSettings;

        public FileManagerController(IOptions<ImageWebApiSettings> webApiSettings)
        {
            this.webApiSettings = webApiSettings.Value;
        }

        public IActionResult Index()
        {
            ViewData["ImageWebApiUrl"] = webApiSettings.BaseUrl;

            return View();
        }

        public IActionResult Upload()
        {
            return Ok();
        }
    }
}