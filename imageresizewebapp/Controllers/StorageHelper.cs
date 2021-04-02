using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Controllers {
    public static class StorageHelper {
        public static bool IsImage(IFormFile file) {
            if (file.ContentType.Contains("image")) {
                return true;
            }
            
            string[] formats = new string[]  { ".jpg", ".png", ".gif", ".jpeg" };
            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzStorageConfig storageConfig) {
            Uri blobUri = new Uri(string.Format(storageConfig.BlobStorageUri, storageConfig.AccountName, storageConfig.ImageContainer, fileName));
            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(storageConfig.AccountName, storageConfig.AccountKey);
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
            await blobClient.UploadAsync(fileStream);
            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzStorageConfig storageConfig) {
            List<string> thumbnailUrls = new List<string>();
            Uri accountUri = new Uri(string.Format(storageConfig.AccountUri, storageConfig.AccountName));
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(storageConfig.ThumbnailContainer);
            if (container.Exists()) {
                foreach(BlobItem blobItem in container.GetBlobs()) {
                    thumbnailUrls.Add($"{container.Uri}/{blobItem.Name}");
                }
            }
            return await Task.FromResult(thumbnailUrls);
        }
    }
}