using System;
using System.Collections.Generic;
using System.Linq;
using CrystalOcean.Data.Models;
using Microsoft.AspNetCore.Mvc;
using CrystalOcean.Data.Models.Repository;
using CrystalOcean.Utils.FileUpload;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CrystalOcean.ImageWebApi.Models;

namespace CrystalOcean.ImageWebApi.Controllers
{
    [Route("images/[controller]")]
    public class UploadController : Controller
    {
        private readonly BinaryRepository _binaryRepository;

        public UploadController(BinaryRepository binaryRepository)
        {
            _binaryRepository = binaryRepository;
        }

        // POST images/upload
        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Index()
        {
            Image img = new Image();
            img.UserId = 1;
            img.Checksum = "blabla";
            img.Width = 100;
            img.Height = 200;

            FormValueProvider formModel = null;
            img = await _binaryRepository.InsertAsync(img, async (stream) => {
                formModel = await Request.StreamFile(stream);
            });

            await _binaryRepository.ExportToFile(img, "/home/sergey/Projects/crystalocean/image.tmp");

            var viewModel = new ImageUploadModel(); 
            var bindingSuccessful = await TryUpdateModelAsync(viewModel, prefix: "", valueProvider: formModel);

            if (!bindingSuccessful)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }

            return Ok(viewModel);
        }
    }
}