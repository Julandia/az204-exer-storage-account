namespace ImageResizeWebApp.Models {
    public class AzStorageConfig {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string BlobStorageUri { get; set;}
        public string AccountUri {get; set;}
        public string ImageContainer { get; set; }
        public string ThumbnailContainer { get; set; }
    }
}