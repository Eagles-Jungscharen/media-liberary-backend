using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EaglesJungscharen.MediaLibrary.Services;
namespace EaglesJungscharen.MediaLibrary {
    public class ResizeImageFunction {
        private PictureBlobStorageService _service;

        public ResizeImageFunction(PictureBlobStorageService service) {
            this._service = service;
        }

        [FunctionName("BlobTriggerResizeImage")]        
        public void Run([BlobTrigger("picture/{pcdid}/{pitemid}/original/{filename}")] Stream myBlob, [Blob("picture/{pcdid}/{pitemid}/tumbnail/{filename}", FileAccess.Write)] CloudBlockBlob imageTumbnail, [Blob("picture/{pcdid}/{pitemid}/album/{filename}", FileAccess.Write)] CloudBlockBlob imageAlbum,string pcdid, string pitemid, string filename, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{pcdid} / {pitemid} / {filename}  \n Size: {myBlob.Length} Bytes");
            try
            {
                if (myBlob != null)
                {
                    var extension = Path.GetExtension(filename);
                    string ct = "";
                    var encoder = GetEncoder(extension,out ct);

                    if (encoder != null)
                    {
                        ResizeImage(myBlob,imageAlbum,1290, encoder, ct);
                        ResizeImage(myBlob,imageTumbnail,150, encoder, ct);
                    }
                    else
                    {
                        log.LogInformation($"No encoder support for: {filename}");
                    }
                }
            } catch (Exception e) {
                log.LogError(e, e.Message);
            }
        }

        private void ResizeImage(Stream input, CloudBlockBlob outputBlob, int maxBound, IImageEncoder encoder, string ct) {
            var output = new MemoryStream();
            using (Image<Rgba32> image = Image.Load<Rgba32>(input))
            {
                if (image.Width > image.Height) {
                    if (maxBound < image.Width) {
                        image.Mutate(x => x.Resize(maxBound, 0));
                    }
                } else {
                    if (maxBound < image.Height) {
                        image.Mutate(x => x.Resize(0,maxBound));
                    }
                }
                image.Save(output, encoder);
                output.Position = 0;
                input.Position = 0;
                outputBlob.Properties.ContentType = ct;
                outputBlob.UploadFromStream(output);
            }
        }

        private static IImageEncoder GetEncoder(string extension, out string contentType)
        {
            IImageEncoder encoder = null;

            extension = extension.Replace(".", "");
            contentType = "application/octet-stream";
            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);

            if (isSupported)
            {
                switch (extension.ToLower())
                {
                    case "png":
                        encoder = new PngEncoder();
                        contentType = "image/png";
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        contentType = "image/jpeg";
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        contentType = "image/jpeg";
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        contentType = "image/gif";
                        break;
                    default:
                        break;
                }
            }

            return encoder;
        }       
    }
}