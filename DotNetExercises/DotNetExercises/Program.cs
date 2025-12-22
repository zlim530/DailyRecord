using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DotNetExercises
{
    internal class Program
    {
        private const string FilePath = "Assets/VsCode.pdf";
        private static readonly DocLib _docNetInstance = DocLib.Instance;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            PDFConvertToImage();
            JPEGImageConvertToPDF();
            GetPDFText();
            GetPDFPageCountAndVersion();
        }

        /// <summary>
        /// 获取 PDF 页数和版本号
        /// </summary>
        public static void GetPDFPageCountAndVersion()
        {
            using var docReader = _docNetInstance.GetDocReader(FilePath, new PageDimensions(1080, 1920));
            var getPageCount = docReader.GetPageCount();
            var getPdfVersion = docReader.GetPdfVersion();
            Console.WriteLine($"PageCount:{getPageCount}, PdfVersion:{getPdfVersion}");
        }

        /// <summary>
        /// 获取 PDF 文本内容
        /// </summary>
        public static void GetPDFText()
        { 
            using var docReader = _docNetInstance.GetDocReader(FilePath, new PageDimensions(1080, 1920));
            using var pageReader = docReader.GetPageReader(0);

            string pageText = pageReader.GetText();

            Console.WriteLine(pageText);
        }

        /// <summary>
        /// 将 JPEG 图片转换为 PDF
        /// </summary>
        public static void JPEGImageConvertToPDF()
        {
            var file = new JpegImage
            {
                Bytes = File.ReadAllBytes("Assets/sample.jpg"),
                Width = 600,
                Height = 800
            };

            var bytes = _docNetInstance.JpegToPdf(new[] { file });

            File.WriteAllBytes("Assets/ConvertedFromJpeg.pdf", bytes);
        }

        /// <summary>
        /// 将 PDF 文件转换为图片
        /// </summary>
        public static void PDFConvertToImage()
        {
            using var docReader = _docNetInstance.GetDocReader(FilePath, new PageDimensions(1080, 1920));
            using var pageReader = docReader.GetPageReader(0);

            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();
            var characters = pageReader.GetCharacters();

            using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            AddBytes(bmp, rawBytes);
            DrawRectangles(bmp, characters);

            using var stream = new MemoryStream();
            bmp.Save("Assets/PageImage.png", ImageFormat.Png);
            File.WriteAllBytes("Assets/PageImage.png", stream.ToArray());
        }

        private static void DrawRectangles(Bitmap bmp, IEnumerable<Character> characters)
        {
            var pen = new Pen(Color.Red);

            using var graphics = Graphics.FromImage(bmp);

            foreach (var c in characters)
            {
                var rect = new Rectangle(c.Box.Left, c.Box.Top, c.Box.Right - c.Box.Left, c.Box.Bottom - c.Box.Top);
                graphics.DrawRectangle(pen, rect);
            }
        }

        private static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}
