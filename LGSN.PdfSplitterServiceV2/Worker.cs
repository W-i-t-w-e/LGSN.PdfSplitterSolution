using LGSN.PdfHandlerLibrary;

namespace LGSN.PdfSplitterServiceV2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly DocLib docLib;
        private readonly string sourcePath;
        private readonly string destinationPath;
        private readonly string archivePath;
        private readonly bool IsArchivingEnabled;
        private readonly bool IsDeleteSourceFileEnabled;
        private readonly string fileExtension;
        private PeriodicTimer timer;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, DocLib docLib)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.docLib = docLib;
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
                        ScanAndSplit(file);
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
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Stopping the PdfSplitterService...");
            await base.StopAsync(cancellationToken);
            logger.LogInformation($"Services stopped at {DateTime.Now}");
            return;
        }

        private void ScanAndSplit(string sourceFilePath)
        {
            logger.LogDebug($"Loading sourcefile {sourceFilePath}...");
            var docInfos = docLib.GetPdfSplitInformation(sourceFilePath);

            SplitDocuments(sourceFilePath, docInfos);
        }

        private void SplitDocuments(string sourceFilePath, Dictionary<string, List<int>> docInfos)
        {

            logger.LogInformation($"Splitting {docInfos.Count} documents...");
            foreach (var document in docInfos)
            {
                SplitDocument(sourceFilePath, document);
            }
        }

        private void SplitDocument(string sourceFilePath, KeyValuePair<string, List<int>> document)
        {
            logger.LogDebug("Creating destination filename...");
            var destFileName = String.Format("{0}.pdf", document.Key);
            var destFilePath = Path.Join(destinationPath, destFileName);
            logger.LogDebug("Creating new pdf file...");
            var fromIndex = document.Value.First();
            var toIndex = document.Value.Last();
            logger.LogDebug($"Adding pages from index {fromIndex} to index {toIndex} to new file {destFileName}");
            var splitBytes = docLib.GetPdfPart(sourceFilePath, fromIndex, toIndex);
            logger.LogInformation($"Storing new file {destFilePath}...");
            File.WriteAllBytes(destFilePath, splitBytes);
        }

        #region File Handling
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
        #endregion
    }
}
