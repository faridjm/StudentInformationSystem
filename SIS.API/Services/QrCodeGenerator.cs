using QRCoder;

namespace SIS.API.Services
{
    public interface IQrCodeGenerator
    {
        /// <summary>Verilmiş mətn üçün PNG QR şəkli (byte[]) yaradır. System.Drawing-dən asılı deyil.</summary>
        byte[] GeneratePng(string content, int pixelsPerModule = 10);
    }

    public class QrCodeGenerator : IQrCodeGenerator
    {
        public byte[] GeneratePng(string content, int pixelsPerModule = 10)
        {
            if (string.IsNullOrWhiteSpace(content)) content = " ";
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            return pngQr.GetGraphic(pixelsPerModule);
        }
    }
}
