using System;
using System.Security.Cryptography;

using Kirkin.Collections.Generic;

namespace Kirkin.Security.Cryptography
{
    internal abstract class ChunkedTransform : ICryptoTransform
    {
        internal const int DefaultChunkSize = 81920; // LOH threshold.

        protected virtual int ChunkSize => DefaultChunkSize;

        public virtual int InputBlockSize => ChunkSize;
        public virtual int OutputBlockSize => ChunkSize;
        public virtual bool CanTransformMultipleBlocks => true;
        public virtual bool CanReuseTransform => false;

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputCount == 0) return 0;

            if (inputCount % InputBlockSize != 0) {
                throw new ArgumentException($"Input count must be a multiple of {InputBlockSize}.");
            }

            int bytesWritten = 0;

            while (inputCount > 0)
            {
                int count = TransformChunk(new ArraySegment<byte>(inputBuffer, inputOffset + bytesWritten, InputBlockSize), outputBuffer, outputOffset);

                bytesWritten += count;
                inputCount -= count;
            }

            return bytesWritten;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputCount == 0) return Array<byte>.Empty;

            if (inputCount > InputBlockSize) {
                throw new ArgumentException($"Input count must be less than or equal to {InputBlockSize}.");
            }

            byte[] output = new byte[OutputBlockSize];
            int bytesWritten = TransformChunk(new ArraySegment<byte>(inputBuffer, inputOffset, inputCount), output, 0);
            
            if (bytesWritten != output.Length) {
                Array.Resize(ref output, bytesWritten);
            }

            return output;
        }

        protected abstract int TransformChunk(in ArraySegment<byte> chunk, byte[] outputBuffer, int outputOffset);

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}