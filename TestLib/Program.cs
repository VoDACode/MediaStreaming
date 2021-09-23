using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MediaStreaming.Core;

namespace TestLib
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] bit2 = null;
            while (true)
            {
                int[,] bit1 = CompressingImage.ImageTo2DByteArray(getScreen());
                if (bit2 == null)
                    bit2 = bit1;
                int[,][,] diffDefault = CompressingImage.Compressing(bit1, bit2);
                byte[] diffComperssing = CompressingImage.ImageDifferenceToByteArray(diffDefault);
                int[,][,] diffUncomperssing = CompressingImage.ArrayToImageDifference(diffComperssing);
                bit2 = bit1;
                Thread.Sleep(33);
            }


        }

        private static Bitmap getScreen()
        {
            var bitmap = new Bitmap(1440, 900);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(1440, 900));
            }
            return bitmap;
        }
    }
}
