using System;
using System.Collections.Generic;
using System.Text;

namespace MediaStreaming.Client.Core.Models
{
    public class BytesList
    {
        public byte[] NewBuffer { get; set; }
        public byte[] OldBuffer { get; set; }

        public void SetBuffer(byte[] arr) => SetBuffer(arr, arr.Length);
        public void SetBuffer(byte[] arr, int count)
        {
            OldBuffer = new byte[NewBuffer.Length];
            Array.Copy(NewBuffer, OldBuffer, OldBuffer.Length);
            NewBuffer = new byte[count];
            Array.Copy(arr, NewBuffer, count);
        }

        public byte[] GetDifference()
        {
            bool resultSizeIsNew = NewBuffer.Length > OldBuffer.Length;
            int size = resultSizeIsNew ? NewBuffer.Length : OldBuffer.Length;

            byte[] difference(int resultSize, int minSize)
            {
                byte[] result = new byte[resultSize];
                for (int i = 0; i < minSize; i++)
                {
                    if (NewBuffer[i] != OldBuffer[i])
                        result[i] = OldBuffer[i];
                }
                return result;
            }

            return difference(size, resultSizeIsNew ? OldBuffer.Length : NewBuffer.Length);
        }

        public void InitArrays(uint size)
        {
            NewBuffer = new byte[size];
            OldBuffer = new byte[size];
        }
    }
}
