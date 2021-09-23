using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaStreaming.Core
{
    public static class CompressingImage
    {
        public static int[,][,] Compressing(int[,] newImage, int[,] oldImage)
        {
            var dImage1 = DividedImage(newImage);
            var dImage2 = DividedImage(oldImage);
            return CreateChangeArray(dImage1, dImage2);
        }

        public static int[,] ImageTo2DByteArray(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            byte[] bytes = new byte[height * data.Stride];
            try
            {
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            int[,] result = new int[height, width];
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    int offset = y * data.Stride + x * 4;
                    byte[] bgra = new byte[] { bytes[offset + 0], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                    result[y, x] = BitConverter.ToInt32(bgra, 0);
                }
            return result;
        }

        public static byte[] ImageDifferenceToByteArray(int[,][,] difference)
        {
            var d = difference.GetLength(0);
            byte countRecord = 0;
            int recordLengthX = 0;
            int recordLengthY = 0;
            for (int x = 0; x < d; x++)
            {
                for (int y = 0; y < d; y++)
                {
                    if (difference[x, y] != null)
                    {
                        countRecord++;
                        recordLengthX = difference[x, y].GetLength(0);
                        recordLengthY = difference[x, y].GetLength(1);
                    }
                }
            }
            /*
             * byte(1) - all count
             * int(2) - recordLengthX
             * int(3) - recordLengthY
             * byte(4) - screen dimension;
             * byte(n + 0 + 1)/ byte(n + 1 + 1) - X/Y
             * int(n + 2 + 1)...int(n + recordLengthX * recordLengthY + 1) - record
             * 255 87 250 46 79
             */
            int oneRecordLenght = (sizeof(byte) + sizeof(byte)) * countRecord +
                sizeof(int) * (recordLengthX * recordLengthY) + sizeof(int);
            byte[] result = new byte[sizeof(byte) + sizeof(int)*2 + oneRecordLenght*countRecord + sizeof(byte)];
            result[0] = countRecord;
            Array.Copy(BitConverter.GetBytes(recordLengthX), 0, result, 1, 4);
            Array.Copy(BitConverter.GetBytes(recordLengthY), 0, result, 5, 4);
            result[9] = (byte)d;
            int lastIndex = 10;
            {
                byte counter = 0;
                int lastSize = 0;
                for (int x = 0; x < d; x++)
                {
                    if (counter > countRecord)
                        break;
                    for (int y = 0; y < d; y++)
                    {
                        if (counter > countRecord)
                            break;
                        if (difference[x, y] != null)
                        {
                            int printSizeIndex = lastIndex;
                            lastIndex += 4;
                            lastSize = lastIndex;
                            result[lastIndex] = (byte)x;
                            lastIndex ++;
                            result[lastIndex] = (byte)y;
                            lastIndex++;
                            int repetitionsCount = 0;
                            int repetition = 0;
                            for (int rx = 0; rx < recordLengthX; rx++)
                            {
                                for (int ry = 0; ry < recordLengthY; ry++)
                                {
                                    if(repetition == 0 && repetitionsCount == 0)
                                        repetition = difference[x, y][rx, ry];
                                    if (ry + 1 >= recordLengthY)
                                    {
                                        if(repetition == difference[x, y][rx, ry])
                                        {
                                            repetitionsCount++;
                                            lastIndex = compresingResult(result, lastIndex, repetition, repetitionsCount);
                                        }
                                        else
                                        {
                                            if (repetitionsCount >= 3)
                                            {
                                                lastIndex = compresingResult(result, lastIndex, repetition, repetitionsCount);
                                            }
                                            else
                                            {
                                                for (int i = 0; i <= repetitionsCount; i++)
                                                {
                                                    Array.Copy(BitConverter.GetBytes(repetition), 0, result, lastIndex, 4);
                                                    lastIndex += 4;
                                                }
                                            }
                                        }
                                        repetitionsCount = 0;
                                        continue;
                                    }
                                    if (repetition == difference[x, y][rx, ry])
                                    {
                                        repetitionsCount++;
                                        continue;
                                    }
                                    else if(repetitionsCount >= 3)
                                    {
                                        lastIndex = compresingResult(result, lastIndex, repetition, repetitionsCount);
                                        repetitionsCount = 0;
                                    }
                                    else if(repetitionsCount < 3)
                                    {
                                        for(int i = 0; i <= repetitionsCount; i++)
                                        {
                                            Array.Copy(BitConverter.GetBytes(repetition), 0, result, lastIndex, 4);
                                            lastIndex += 4;
                                        }
                                        repetitionsCount = 0;
                                    }
                                    repetition = difference[x, y][rx, ry];
                                }
                            }
                            lastSize = lastIndex - lastSize;
                            Array.Copy(BitConverter.GetBytes(lastSize), 0, result, printSizeIndex, 4);
                            counter++;
                        }
                    }
                }
            }
            byte[] tmp = new byte[lastIndex];
            Array.Copy(result, tmp, lastIndex);
            return tmp;
        }

        public static int[,][,] ArrayToImageDifference(byte[] arr)
        {
            byte dimension = arr[9];
            int recordLengthX = BitConverter.ToInt32(arr, 1);
            int recordLengthY = BitConverter.ToInt32(arr, 5);
            int[,][,] result = new int[dimension, dimension][,];
            int readSize = 10;
            for(int r = 0; r < arr[0]; r++)
            {
                int writeBites = 0;
                byte[] tmp = new byte[recordLengthX * recordLengthY * 4];

                int size = BitConverter.ToInt32(arr, readSize);
                byte xPos = arr[readSize + 4];
                byte yPos = arr[readSize + 5];
                readSize += 6;
                result[xPos, yPos] = new int[recordLengthX, recordLengthY];
                for (int i = 0; i < size; i++)
                {
                    if (i + readSize >= arr.Length || writeBites >= tmp.Length)
                        break;
                    if (checkCompressRecord(arr, readSize + i))
                    {
                        var decompress = decompressingResult(arr, readSize + i);
                        Array.Copy(decompress, 0, tmp, writeBites, decompress.Length);
                        writeBites += decompress.Length;
                        i += 17;
                    }
                    else
                    {

                        tmp[writeBites] = arr[i + readSize];
                        writeBites++;
                    }
                }
                int xTmp = 0;
                int yTmp = 0;
                for (int i = 0; i < tmp.Length; i++)
                {
                    result[xPos, yPos][xTmp, yTmp] = BitConverter.ToInt32(tmp, i);
                    i += 3;
                    yTmp++;
                    if (yTmp == recordLengthY )
                    {
                        yTmp = 0;
                        xTmp++;
                        if (xTmp == recordLengthX)
                            break;
                    }
                }
                readSize += size - 2;
            }
            return result;
        }

        public static Bitmap GetScreen(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(width, height));
            }
            return bitmap;
        }
        #region Compresing result
        private static byte[] decompressingResult(byte[] arr, int indexCompression)
        {
            int count = BitConverter.ToInt32(arr, indexCompression + 5);
            byte[] result = new byte[count*4];
            byte[] repetition = new byte[] { arr[indexCompression + 9], arr[indexCompression + 10], arr[indexCompression + 11], arr[indexCompression + 12] };
            for (int i = 0; i < count; i++)
            {
                Array.Copy(repetition, 0, result, i * 4, 4);
            }
            return result;
        }
        private static bool checkCompressRecord(byte[] arr, int index)
        {
            byte[] startCompresing = new byte[] { 255, 87, 250, 46, 255 };
            if (index + startCompresing.Length > arr.Length)
                return false;
            for(int i = 0; i < startCompresing.Length; i++)
            {
                if (arr[index + i] != startCompresing[i])
                    return false;
            }
            return true;
        }
        private static int compresingResult(byte[] result, int lastIndex, int repetition, int repetitionsCount)
        {
            //255 87 250 46 255
            Array.Copy(new byte[] { 255, 87, 250, 46, 255 }, 0, result, lastIndex, 5);
            lastIndex += 5;
            Array.Copy(BitConverter.GetBytes(repetitionsCount), 0, result, lastIndex, 4);
            lastIndex += 4;
            Array.Copy(BitConverter.GetBytes(repetition), 0, result, lastIndex, 4);
            lastIndex += 4;
            Array.Copy(new byte[] { 55, 87, 50, 46, 55 }, 0, result, lastIndex, 5);
            lastIndex += 5;
            return lastIndex;
        }
        #endregion

        #region CompressingImage
        private static int[,][,] CreateChangeArray(int[,][,] lastSectors, int[,][,] nextSectors)
        {
            var sectorsCount = lastSectors.GetLength(0);
            int[,][,] res = new int[sectorsCount, sectorsCount][,];
            for (int x = 0; x < sectorsCount; x++)
            {
                for (int y = 0; y < sectorsCount; y++)
                {
                    if (!cheakEquals(nextSectors[x, y], lastSectors[x, y]))
                    {
                        res[x, y] = nextSectors[x, y];
                    }
                    else
                    {
                        res[x, y] = null;
                    }
                }
            }
            return res;
        }

        private static bool cheakEquals(int[,] arr1, int[,] arr2)
        {
            for (int x = 0; x < arr1.GetLength(0); x++)
            {
                for (int y = 0; y < arr1.GetLength(0); y++)
                {
                    if (arr1[x, y] != arr2[x, y])
                        return false;
                }
            }
            return true;
        }

        private static int[,][,] DividedImage(int[,] img)
        {
            const int sectorsCount = 8 / 2;
            int[,][,] sectors = new int[sectorsCount, sectorsCount][,];
            for (int y = 0; y < sectorsCount; y++)
            {
                for (int x = 0; x < sectorsCount; x++)
                {
                    var xSize = img.GetLength(0) / sectorsCount;
                    var ySize = img.GetLength(1) / sectorsCount;
                    sectors[x, y] = new int[xSize, ySize];
                    fillArr(x, y, img, sectors, sectorsCount);
                }
            }
            return sectors;
        }

        private static void fillArr(int x, int y, int[,] img, int[,][,] sectors, int sectorsCount)
        {
            var xSize = img.GetLength(0) / sectorsCount;
            var ySize = img.GetLength(1) / sectorsCount;
            for (int xi = x * xSize, xs = 0; xs < xSize; xi++, xs++)
            {
                for (int yi = y * ySize, ys = 0; ys < ySize; yi++, ys++)
                {
                    sectors[x, y][xs, ys] = img[xi, yi];
                }
            }
        }
        #endregion
    }
}
