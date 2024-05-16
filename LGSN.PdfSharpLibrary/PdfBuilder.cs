using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace LGSN.PdfSharpLibrary
{
    public class PdfBuilder
    {
        private readonly ILogger<PdfBuilder> logger;

        public PdfBuilder(ILogger<PdfBuilder> logger)
        {
            this.logger = logger;
        }
        public void BuildDocuments(string sourceFilePath, string destinationPath, Dictionary<string, List<int>> documents)
        {
            logger.LogInformation($"Opening pdf file {sourceFilePath}...");
            var inputFile = OpenPdfFile(sourceFilePath);
            logger.LogInformation($"Splitting {documents.Count} documents...");
            SplitDocument(inputFile, documents, destinationPath);
        }

        private static PdfDocument OpenPdfFile(string sourceFilePath)
        {
            return PdfReader.Open(sourceFilePath, PdfDocumentOpenMode.Import);           
        }

        private void SplitDocument(PdfDocument sourceFile, Dictionary<string, List<int>> documents, string destinationPath)
        {
            var destFileExtension = Path.GetExtension(sourceFile.FullPath);
            foreach (var document in documents)
            {
                logger.LogDebug("Creating destination filename...");
                var destFileName = string.Format("{0}{1}", document.Key, destFileExtension);
                var destFilePath = Path.Join(destinationPath, destFileName);
                logger.LogDebug("Creating new pdf file...");
                var outputDocument = new PdfDocument { Version = sourceFile.Version };
                foreach (var index in document.Value)
                {
                    logger.LogDebug($"Adding page index {index} from {sourceFile.FullPath} to new file {destFilePath}");
                    outputDocument.AddPage(sourceFile.Pages[index]);
                }
                logger.LogInformation($"Storing new file {destFilePath}...");
                outputDocument.Save(destFilePath);
            }
        }
    }
}
