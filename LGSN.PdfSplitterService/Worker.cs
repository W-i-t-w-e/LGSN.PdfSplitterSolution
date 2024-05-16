using LGSN.PdfSplitterService.Services;

namespace LGSN.PdfSplitterService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly SplitterService splitterService;
        private readonly string sourcePath;
        private readonly string destinationPath;
        private readonly string archivePath;
        private readonly bool IsArchivingEnabled;
        private readonly bool IsDeleteSourceFileEnabled;
        private readonly string fileExtension;
        private PeriodicTimer timer;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, SplitterService splitterService)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.splitterService = splitterService;
            sourcePath = configuration.GetValue<string>("Path:SourcePath")!;
            destinationPath = configuration.GetValue<string>("Path:DestinationPath")!;
            archivePath = configuration.GetValue<string>("Path:ArchivePath")!;
            fileExtension = "pdf";
            IsArchivingEnabled = configuration.GetValue<bool>("Common:ArchivingEnabled");
            IsDeleteSourceFileEnabled = configuration.GetValue<bool>("Common:DeleteSourceFile");
            var timeSpan = configuration.GetValue<TimeSpan>("Common:Interval");
            timer = new(timeSpan);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken) && stoppingToken.IsCancellationRequested == false)
                {
                    logger.LogInformation($"Getting all {fileExtension} files from {sourcePath}...");
                    var fileList = GetAllPdfFiles(sourcePath);
                    foreach (var file in fileList)
                    {
                        splitterService.Convert(file, destinationPath);
                        if (IsArchivingEnabled)
                        {
                            logger.LogInformation($"Archiving source file {file} to {archivePath}...");
                            MoveFile(file, archivePath);
                            continue;
                        }
                        if (IsDeleteSourceFileEnabled)
                        {
                            logger.LogInformation($"Deleting source file {file}...");
                            DeleteFile(file);
                        }
                    }

                    logger.LogInformation("Done...");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                hostApplicationLifetime.StopApplication();
            }
            
        }

        private string[] GetAllPdfFiles(string sourcePath)
        {
            var searchPattern = string.Format("*.{0}", fileExtension);
            var files = Directory.GetFiles(sourcePath, searchPattern, SearchOption.TopDirectoryOnly);
            logger.LogInformation($"{files.Length} files found...");
            return files;
        }

        private void MoveFile(string sourceFile, string destinationPath)
        {
            try
            {
                var destFileName = Path.GetFileName(sourceFile);
                var destFilePath = Path.Join(destinationPath, destFileName);
                File.Move(sourceFile, destFilePath);
            }
            catch (Exception)
            {
                logger.LogError($"Error moving file {sourceFile} to destinationPath {destinationPath}");
                throw;
            }
            
        }
        private void DeleteFile(string sourceFile)
        {
            try
            {
                File.Delete(sourceFile);
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    logger.LogWarning($"Error deleting file {sourceFile}!");
                    logger.LogWarning($"Try to update file attribute...");
                    File.SetAttributes(sourceFile, FileAttributes.Normal);
                    File.Delete(sourceFile);
                    logger.LogInformation("File successfully deleted.");
                }
                catch (Exception)
                {
                    logger.LogError($"Unable to delete file {sourceFile}!");
                    throw;
                }
                
            }
            catch (Exception)
            {
                logger.LogError($"Error deleting file {sourceFile}");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Stopping the PdfSplitterService...");
            await base.StopAsync(cancellationToken);
            logger.LogInformation($"Services stopped at {DateTime.Now}");
            return;
        }
    }
}
