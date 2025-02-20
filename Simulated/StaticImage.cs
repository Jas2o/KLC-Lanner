using System;
using System.Drawing;
using static LibKaseya.Enums;

namespace KLC_Hawk {
    public static class StaticImage {

        public static void ServerConnected(WsY2 sender) {
            sender.Send(new byte[] { 0x27 });

            string screenLayoutExample3 = @"{""default_screen"":131073,""screens"":[{""screen_height"":900,""screen_id"":131073,""screen_name"":""\\\\.\\DISPLAY1"",""screen_width"":1600,""screen_x"":0,""screen_y"":0},{""screen_height"":1080,""screen_id"":1245327,""screen_name"":""\\\\.\\DISPLAY2"",""screen_width"":1920,""screen_x"":1615,""screen_y"":-741},{""screen_height"":1080,""screen_id"":196759,""screen_name"":""\\\\.\\DISPLAY3"",""screen_width"":1920,""screen_x"":-305,""screen_y"":-1080}]}";

            byte[] bHostDesktopConfig = MITM.GetJsonMessage(KaseyaMessageTypes.HostDesktopConfiguration, screenLayoutExample3);
            sender.Send(bHostDesktopConfig);
        }

        public static void MessageReceived(WsY2 sender, dynamic json) {
            int width = (int)json["width"];
            int height = (int)json["height"];
            width = Math.Clamp(width, 1, 1920); //Dirty
            height = Math.Clamp(height, 1, 1080);

            byte[] bThumbnailResultHeader = MITM.GetJsonMessage(KaseyaMessageTypes.ThumbnailResult, "{}");

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int randomColor = sender.random.Next(0, 5);
            using (Graphics g = Graphics.FromImage(bitmap)) {
                if (randomColor == 0)
                    g.Clear(System.Drawing.Color.Black);
                else if (randomColor == 1)
                    g.Clear(System.Drawing.Color.DarkGreen);
                else if (randomColor == 2)
                    g.Clear(System.Drawing.Color.SteelBlue);
                else if (randomColor == 3)
                    g.Clear(System.Drawing.Color.LightSalmon);
                else if (randomColor == 4)
                    g.Clear(System.Drawing.Color.DimGray);
                else if (randomColor == 5)
                    g.Clear(System.Drawing.Color.Orchid);
            }
            //--
            ImageConverter converter = new ImageConverter();
            byte[] bitmapdata = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            //--
            bitmap.Dispose();

            byte[] bThumbnailResultFull = new byte[bThumbnailResultHeader.Length + bitmapdata.Length];
            Array.Copy(bThumbnailResultHeader, 0, bThumbnailResultFull, 0, bThumbnailResultHeader.Length);
            Array.Copy(bitmapdata, 0, bThumbnailResultFull, bThumbnailResultHeader.Length, bitmapdata.Length);

            sender.Send(bThumbnailResultFull);
        }

    }
}
