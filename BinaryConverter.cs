using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LIFi_Emulator
{
    internal class BinaryConverter
    {

        public string[] TextToBinary(string text)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(text);
            string[] result = new string[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                result[i] = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
            return result;
        }

        public List<string[]> ImageToBinary(string imgPath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imgPath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);

            // Проверяем формат пикселей (обычно Bgra32 в WPF)
            if (writableBitmap.Format != PixelFormats.Bgra32)
            {
                writableBitmap = new WriteableBitmap(new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, null, 0));
            }

            int bytesPerPixel = (writableBitmap.Format.BitsPerPixel + 7) / 8; // Обычно 4 (BGRA)
            int stride = writableBitmap.PixelWidth * bytesPerPixel;
            byte[] pixelData = new byte[writableBitmap.PixelHeight * stride];

            writableBitmap.CopyPixels(pixelData, stride, 0);

            List<string[]> binaryPixels = new List<string[]>();

            for (int y = 0; y < writableBitmap.PixelHeight; y++)
            {
                for (int x = 0; x < writableBitmap.PixelWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;

                    byte b = pixelData[index];     // Синий
                    byte g = pixelData[index + 1]; // Зелёный
                    byte r = pixelData[index + 2]; // Красный
                                                   // byte a = pixelData[index + 3]; // Альфа (прозрачность)

                    // Переводим цвет в бинарный вид (RGB)
                    string rBinary = Convert.ToString(r, 2).PadLeft(8, '0');
                    string gBinary = Convert.ToString(g, 2).PadLeft(8, '0');
                    string bBinary = Convert.ToString(b, 2).PadLeft(8, '0');

                    string[] rgb = new string[3] { rBinary, gBinary, bBinary };
                    binaryPixels.Add(rgb); // 24 бита на пиксель
                }
            }

            return binaryPixels;
        }
    }
}
