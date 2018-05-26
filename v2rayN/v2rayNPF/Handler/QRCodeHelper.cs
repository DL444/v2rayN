using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.QrCode;
using System.Windows.Media;

namespace v2rayNPF.Handler
{
    /// <summary>
    /// 含有QR码的描述类和包装编码和渲染
    /// </summary>
    public class QRCodeHelper
    {
        public static DrawingImage GetQRCode(string strContent)
        {
            ////Image img = null;
            //try
            //{
            //    QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            //    options.CharacterSet = "UTF-8";
            //    options.DisableECI = true; // Extended Channel Interpretation (ECI) 主要用于特殊的字符集。并不是所有的扫描器都支持这种编码。
            //    options.ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M; // 纠错级别
            //    options.Width = 500;
            //    options.Height = 500;
            //    options.Margin = 1;
            //    // options.Hints，更多属性，也可以在这里添加。

            //    BarcodeWriter writer = new BarcodeWriter();
            //    writer.Format = BarcodeFormat.QR_CODE;
            //    writer.Options = options;
            //    return writer.Write(strContent);
            //    //Bitmap bmp = writer.Write(strContent);
            //    //img = (Image)bmp;
            //    //return img;
            //}
            //catch
            //{
            //    return null;
            //    //return img;
            //}
            if (!strContent.IsNullOrEmpty())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrData = qrGenerator.CreateQrCode(strContent, QRCodeGenerator.ECCLevel.M);
                XamlQRCode qRCode = new XamlQRCode(qrData);
                return qRCode.GetGraphic(20);
            }
            return null;
        }
    }
}
