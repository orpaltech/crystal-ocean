using System;
using System.Collections.Generic;
using System.Linq;
using CrystalOcean.Data.Models;
using Microsoft.AspNetCore.Mvc;
using CrystalOcean.Data.Repository;
using CrystalOcean.Utils.FileUpload;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CrystalOcean.ImageWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;

namespace CrystalOcean.ImageWebApi.Controllers
{
    /*[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]*/
    [Route("/api/[controller]")]
    public class UploadController : Controller
    {
        private readonly BinaryRepository _binaryRepository;

        public UploadController(BinaryRepository binaryRepository)
        {
            _binaryRepository = binaryRepository;
        }

        // POST /api/upload
        [HttpPost]
        [DisableFormValueModelBinding] 
        public async Task<IActionResult> Index()
        {
            if (ModelState.IsValid)
            {
                FormValueProvider valueProvider = null;
                var viewModel = new ImageUploadModel(); 
                bool updateResult = false;
                Image binary;

                binary = await _binaryRepository.InsertAsync(
                    async (stream) => 
                    { 
                        valueProvider = await Request.StreamFile(stream);
                    },
                    async () => 
                    {
                        updateResult = await TryUpdateModelAsync(
                            viewModel, prefix: "", valueProvider: valueProvider);
                        if (updateResult) 
                        {
                            // TODO: read image information from request
                            // TODO: get image dimensions, etc
                            binary = new Image();
                            binary.UserId = 4;
                            binary.Checksum = viewModel.Checksum;
                            binary.Width = 100;
                            binary.Height = 200;
                            return binary;
                        }
                        else 
                            return null;
                    }
                );

                //await _binaryRepository.ExportToFileAsync(img, "/home/sergey/Projects/crystalocean/image.tmp");

                if (updateResult)
                    return Ok(viewModel);
            }

            return BadRequest(ModelState);
        }
    }
}