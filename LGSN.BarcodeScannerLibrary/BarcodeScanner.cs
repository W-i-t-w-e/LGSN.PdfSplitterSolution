using System.Drawing;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;


namespace LGSN.BarcodeScannerLibrary
{
    public static class BarcodeScanner
    {
        public static string GetBarcode(Image input)
        {
            using (var bmp = new Bitmap(input))
            {
                int height = bmp.Height;
                int width = bmp.Width;
                int splitY = 3;
                int cropY = bmp.Height / splitY;
                var startY = 0;

                for (int i = 0; i < splitY; i++)
                {
                    // stick within imagesize
                    if (startY + cropY > height)
                    {
                        cropY = height;
                    }
                    var rect = new Rectangle(0, startY, width, cropY);
                    using (var imgPart = bmp.Clone(rect, bmp.PixelFormat))
                    {
                        var barcode = GetBarcodeFromImage(imgPart);
                        if (string.IsNullOrEmpty(barcode) == false)
                        {
                            return barcode;
                        }
                        startY += cropY;
                    }
                }
                return string.Empty;
            }

        }

        private static string GetBarcodeFromImage(Bitmap imgPart)
        {
            var reader = new BarcodeReader()
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128 }
                }
            };
            Result result = reader.Decode(imgPart);
            if (result is null)
            {
                return string.Empty;
            }
            return result.Text;
        }
    }
}
