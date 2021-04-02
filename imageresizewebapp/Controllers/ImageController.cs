using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Controllers {
    [Route("api/[controller]")]
    public class ImagesController : Controller {

        private readonly AzStorageConfig StorageConfig = null;

        public ImagesController(IOptions<AzStorageConfig> config) {
            StorageConfig = config.Value;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files) {
            bool isUploaded = false;
            try {
                if (files.Count == 0) {
                    return BadRequest("No files received from the upload.");
                }
                if (string.IsNullOrEmpty(StorageConfig.AccountKey) || string.IsNullOrEmpty(StorageConfig.AccountName)) {
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");
                }
                if (string.IsNullOrEmpty(StorageConfig.ImageContainer)) {
                    return BadRequest("Please provide a name for your image container in the azure blob storage");
                }
                foreach(var formFile in files) {
                    if (StorageHelper.IsImage(formFile)) {
                        if (formFile.Length > 0) {
                            using(Stream stream = formFile.OpenReadStream()) {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, StorageConfig);
                            }
                        }
                    } else {
                        return new UnsupportedMediaTypeResult();
                    }
                }
                if (isUploaded) {
                    if (!string.IsNullOrEmpty(StorageConfig.ThumbnailContainer)) {
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    } else {
                        return new AcceptedResult();
                    }
                } else {
                    return BadRequest("Look like the image couldn't upload to the storage");
                }
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails() {
            try {
                if (string.IsNullOrEmpty(StorageConfig.AccountKey) || string.IsNullOrEmpty(StorageConfig.AccountName)) {
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");
                }

                if (string.IsNullOrEmpty(StorageConfig.ImageContainer)) {
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");
                }
                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(StorageConfig);
                return new ObjectResult(thumbnailUrls);
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}