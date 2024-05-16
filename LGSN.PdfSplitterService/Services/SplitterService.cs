using LGSN.BarcodeScannerLibrary;
using LGSN.PdfSharpLibrary;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGSN.PdfSplitterService.Services
{
    public class SplitterService
    {
        private readonly Dictionary<string, List<int>> documents = [];
        private readonly ILogger<SplitterService> logger;
        private readonly PdfBuilder pdfBuilder;

        public SplitterService(ILogger<SplitterService> logger, PdfBuilder pdfBuilder)
        {
            this.logger = logger;
            this.pdfBuilder = pdfBuilder;
        }
        public void Convert(string sourceFilePath, string destinationPath)
        {
            logger.LogDebug($"Loading sourcefile {sourceFilePath}...");
            using (var pdfDocument = PdfDocument.Load(sourceFilePath))
            {
                for (int i = 0; i < pdfDocument.PageCount; i++)
                {
                    logger.LogDebug($"Rendering image from page index {i}");
                    using (var bitmapImage = pdfDocument.Render(i, 300, 300, PdfRenderFlags.CorrectFromDpi))
                    {
                        logger.LogDebug("Try to get code128 barcode from page image...");
                        var barcode = BarcodeScanner.GetBarcode(bitmapImage);

                        if (string.IsNullOrEmpty(barcode))
                        {
                            logger.LogDebug("No barcode found...");
                            if (documents.Count == 0)
                            {
                                var fileName = DateTime.Now.ToString("yyyyMMddhhmmssff");
                                logger.LogDebug($"Adding new document {fileName} to dictionary...");
                                documents.Add(fileName, []);
                            }
                            // Add index to last entry of dictionary
                            logger.LogDebug("Adding page index to last document in dictionary...");
                            documents.Last().Value.Add(i);
                            continue;
                        }

                        if (documents.ContainsKey(barcode) == false)
                        {
                            logger.LogDebug($"Adding new document {barcode} to dictionary...");
                            documents.Add(barcode, []);
                        }
                        // Add new entry to dictionary with key as barcode and a new list<int> as value
                        logger.LogDebug($"Adding page index to {barcode} in dictionary...");
                        documents[barcode].Add(i);
                    };
                }
                pdfBuilder.BuildDocuments(sourceFilePath, destinationPath, documents);
                logger.LogDebug("Cleanup dictionary...");
                documents.Clear();
            }                
        }
    }
}
