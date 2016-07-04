namespace ImageDis
{
    public static class ImageTypes
    {
        public static readonly ImageType Jpg = new ImageType
        {
            ContentType = "image/jpg",
            FileExtension = "jpg"
        };

        public static readonly ImageType Jpeg = new ImageType
        {
            ContentType = "image/jpeg",
            FileExtension = "jpg"
        };

        public static readonly ImageType Png = new ImageType
        {
            ContentType = "image/png",
            FileExtension = "png"
        };
    }

    public struct ImageType
    {
        public string ContentType { get; set; }

        public string FileExtension { get; set; }
    }
}
