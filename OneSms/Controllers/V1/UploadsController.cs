using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Responses;
using OneSms.Extensions;
using OneSms.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUriService _uriService;

        public UploadsController(IWebHostEnvironment environment, IUriService uriService)
        {
            _environment = environment;
            _uriService = uriService;
        }

        [HttpPost(ApiRoutes.Upload.Image)]
        public async Task<IActionResult> UploadImage(IFormFile formImage)
        {
            var imgFolder = Path.Combine(_environment.WebRootPath, $"uploads/images");
            if (!Directory.Exists(imgFolder))
                Directory.CreateDirectory(imgFolder);
            if (formImage.IsImage())
            {
                var fileNameArray = formImage.FileName.Split(".");
                var fileName = $"{fileNameArray.FirstOrDefault()}{DateTime.UtcNow.Ticks}.{fileNameArray.LastOrDefault() ?? "png"}";
                using var fileStream = new FileStream(Path.Combine(imgFolder, fileName), FileMode.OpenOrCreate);
                await formImage.CopyToAsync(fileStream);

                var filePath = fileStream.Name.Replace(_environment.WebRootPath, _uriService.InternetUrl);
                var path = filePath.Replace("\\", "/");

                return Created(path, new FileUploadSuccessReponse { Url = path });
            }
            return BadRequest(new FileUploadFailedResponse { Errors = new List<string> { "Not valid image" } });
        }
    }
}
