using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        private readonly LocalMailService _mailService;


        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider, LocalMailService mailService)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }

        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            //FileContentResult
            //FileStreamResult
            //PhysicalFileResult
            //VirtualFileResult

            var pathToFile = "diet-plan.pdf";

            if(!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }

            if(!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
            {
                contentType = "application/octet-stream"; //Setting default media type of arbitrary binary data
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);

            //return File(bytes, "text/plain", Path.GetFileName(pathToFile)); - Will work but sends a blank PDF

            return File(bytes, contentType, Path.GetFileName(pathToFile));

        }
    }
}
