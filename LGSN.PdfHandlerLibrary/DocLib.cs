using Docnet.Core.Models;
using Docnet.Core.Readers;
using LGSN.BarcodeScannerLibrary;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LGSN.PdfHandlerLibrary
{
    public class DocLib
    {
        private readonly ILogger<DocLib> logger;

        public DocLib(ILogger<DocLib> logger)
        {
            this.logger = logger;
        }
        public byte[] GetPdfPart(string sourceFilePath, int fromIndex, int toIndex)
        {
            using (var docLib = Docnet.Core.DocLib.Instance)
            {
                return docLib.Split(sourceFilePath, fromIndex, toIndex);
            }
        }
        public Dictionary<string, List<int>> GetPdfSplitInformation(string sourceFilePath)
        {
            Dictionary<string, List<int>> output = [];
            using (var docLib = Docnet.Core.DocLib.Instance)
            {
                using (var docReader = docLib.GetDocReader(sourceFilePath, new PageDimensions(5)))
                {
                    for (int i = 0; i < docReader.GetPageCount(); i++)
                    {
                        logger.LogDebug($"Get image from page index {i}");
                        using (var bmp = GetImageFromPage(docReader, i))
                        {
                            logger.LogDebug("Try to get code128 barcode from page image...");
                            var barcode = BarcodeScanner.GetBarcodeFromBitmapMorePrecise(bmp);

                            if (string.IsNullOrEmpty(barcode))
                            {
                                logger.LogDebug("No barcode found...");
                                if (output.Count == 0)
                                {
                                    var fileName = DateTime.Now.ToString("yyyyMMddhhmmssff");
                                    logger.LogDebug($"Adding new document {fileName} to dictionary...");
                                    output.Add(fileName, []);
                                }
                                logger.LogDebug("Adding page index to last document in dictionary...");
                                output.Last().Value.Add(i);
                                continue;
                            }

                            if (output.ContainsKey(barcode) == false)
                            {
                                logger.LogDebug($"Adding new document {barcode} to dictionary...");
                                output.Add(barcode, []);
                            }
                            // Add new entry to dictionary with key as barcode and a new list<int> as value
                            logger.LogDebug($"Adding page index to {barcode} in dictionary...");
                            output[barcode].Add(i);
                        }
                    }
                }
            }
            return output;
        }

        private static Bitmap GetImageFromPage(IDocReader docReader, int index)
        {
            using (var pageReader = docReader.GetPageReader(index))
            {
                var rawBytes = pageReader.GetImage();
                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();
                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                AddBytes(bmp, rawBytes);
                return bmp;                
            }
        }

        private static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}
