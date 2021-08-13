using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MediaStreamingClientCore.Helpers
{
    class ScreenBits
    {
        private byte[,,] newBits;
        private byte[,,] oldBits;
        public int Position { get; set; }
        public int Width { get; }
        public int Height { get; }
        public byte[,,] NewBits { get => newBits; }
        public byte[,,] OldBits { get => oldBits; }
        public ScreenBits(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new IndexOutOfRangeException();
            newBits = new byte[4, width, height];
            oldBits = new byte[4, width, height];
            Width = width;
            Height = height;
            FillBuffer(Color.Black);
        }

        public void FillBuffer(Color val)
        {
            for (int w = 0; w < Width; w++)
                for (int h = 0; h < Height; h++)
                        WriteBit(val, w, h);
        }

        public void WriteBit(Color val, int width, int height)
        {
            if (width < 0 || height < 0)
                throw new IndexOutOfRangeException();
            newBits[0, width, height] = val.R;
            newBits[1, width, height] = val.G;
            newBits[2, width, height] = val.B;
            newBits[3, width, height] = val.A;
        }

        public byte[,,] GetDifference()
        {
            byte[,,] differences = new byte[4, Width, Height];
            for (int w = 0; w < Width; w++)
                for (int h = 0; h < Height; h++)
                    for (int bpp = 0; bpp < 4; bpp++)
                        if (NewBits[bpp, w, h] != OldBits[bpp, w, h])
                       {
                            differences[bpp, w, h] = NewBits[bpp, w, h];
                            OldBits[bpp, w, h] = NewBits[bpp, w, h];
                       }
            return differences;
        }
    }
}
