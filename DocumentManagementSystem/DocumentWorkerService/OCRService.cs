using System;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;
using Tesseract;
using log4net;

namespace DocumentWorkerService
{
    public class OCRService
    {
        private readonly IMinioClient _minioClient;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OCRService));
        private readonly string _bucketName = "uploads"; // Specify your MinIO bucket name here

        public OCRService(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public async Task<string> PerformOCRAsync(string filePath)
        {
            string ocrText = string.Empty;

            try
            {
                string file = Path.GetFileName(filePath.Replace("minio://", ""));
                
                _logger.Info($"Performing OCR on file {file}");

                // Download file from MinIO
                var document = await DownloadFileFromMinIOAsync(file);

                // Convert PDF to image using Ghostscript
                string imagePath = ConvertPdfToImage(document);
                
                _logger.Info($"Image path after conversion: {imagePath}");
                
                _logger.Info($"TESSDATA_ENVIRONMENT: {Environment.GetEnvironmentVariable("TESSDATA_PREFIX")}");
                Environment.SetEnvironmentVariable("TESSDATA_PREFIX", "/app/tessdata", EnvironmentVariableTarget.Process);

                string outputFile = "/tmp/output";
                string realOutputFile = "/tmp/output.txt";
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tesseract",
                    Arguments = $"{imagePath} {outputFile}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var process = System.Diagnostics.Process.Start(processStartInfo);
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    string ocrText1 = File.ReadAllText(realOutputFile);
                    _logger.Info($"OCR Text: {ocrText1}");
                }
                else
                {
                    string error = process.StandardError.ReadToEnd();
                    _logger.Error($"Tesseract Error: {error}");
                }

                
                // Clean up the image after processing
                File.Delete(imagePath);
                // Clean up the downloaded file after processing
                File.Delete(document);

                _logger.Info($"OCR processing completed for file {file}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error performing OCR: {ex.Message}");
            }
            
            _logger.Info($"OCR Text: {ocrText}");
            return ocrText;
        }

        private async Task<string> DownloadFileFromMinIOAsync(string fileName)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                // Download the file from MinIO
                await _minioClient.GetObjectAsync(new GetObjectArgs().WithBucket(_bucketName).WithObject(fileName).WithCallbackStream(stream =>
                {
                    using (var fileStream = File.Create(tempFilePath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }));

                _logger.Info($"File {fileName} downloaded from MinIO to {tempFilePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error downloading file from MinIO: {ex.Message}");
                throw;
            }

            return tempFilePath;
        }

        private string ConvertPdfToImage(string pdfFilePath)
        {
            string imagePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(pdfFilePath) + ".png");

            try
            {
                // Use Ghostscript to convert PDF to image (PNG in this case)
                var gsArgs = $"-dNOPAUSE -dBATCH -sDEVICE=pngalpha -r300 -sOutputFile={imagePath} {pdfFilePath}";
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "gs";  // Ghostscript command
                process.StartInfo.Arguments = gsArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    _logger.Error($"Ghostscript error: {error}");
                    throw new Exception("Ghostscript failed to convert PDF to image.");
                }

                _logger.Info($"PDF converted to image: {imagePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error converting PDF to image: {ex.Message}");
                throw;
            }

            return imagePath;
        }
    }
}
