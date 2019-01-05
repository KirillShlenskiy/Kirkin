using System;
using System.Security.Cryptography;

using Kirkin.Collections.Generic;

namespace Kirkin.Security.Cryptography
{
    partial class SymmetricAlgorithm
    {
        private const int LOHThreshold = 81920;

        sealed class SymmetricAlgorithmEncryptTransform : ICryptoTransform
        {
            public SymmetricAlgorithm Algorithm { get; private set; }

            public int InputBlockSize
            {
                get
                {
                    // MaxEncryptOutputBufferSize of an empty array represents pure overhead.
                    return LOHThreshold - Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);
                }
            }

            public int OutputBlockSize
            {
                get
                {
                    return LOHThreshold;
                }
            }

            public bool CanTransformMultipleBlocks
            {
                get
                {
                    return true;
                }
            }

            public bool CanReuseTransform
            {
                get
                {
                    return false;
                }
            }

            internal SymmetricAlgorithmEncryptTransform(SymmetricAlgorithm algorithm)
            {
                Algorithm = algorithm;
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (inputCount == 0) return 0;

                if (inputCount % InputBlockSize != 0) {
                    throw new ArgumentException($"Input count must be a multiple of {InputBlockSize}.");
                }

                int bytesWritten = 0;

                while (inputCount > 0)
                {
                    int count = Algorithm.EncryptBytes(new ArraySegment<byte>(inputBuffer, inputOffset + bytesWritten, InputBlockSize), outputBuffer, outputOffset);

                    bytesWritten += count;
                    inputCount -= count;
                }

                return bytesWritten;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputCount > InputBlockSize) {
                    throw new ArgumentException($"Input count must be less than or equal to {InputBlockSize}.");
                }

                byte[] output = new byte[OutputBlockSize];
                int bytesWritten = Algorithm.EncryptBytes(new ArraySegment<byte>(inputBuffer, inputOffset, inputCount), output, 0);
            
                if (bytesWritten != output.Length) {
                    Array.Resize(ref output, bytesWritten);
                }

                return output;
            }

            public void Dispose()
            {
                Algorithm = null;
            }
        }

        sealed class SymmetricAlgorithmDecryptTransform : ICryptoTransform
        {
            public SymmetricAlgorithm Algorithm { get; private set; }

            public int InputBlockSize
            {
                get
                {
                    return LOHThreshold;
                }
            }

            public int OutputBlockSize
            {
                get
                {
                    // MaxEncryptOutputBufferSize of an empty array represents pure overhead.
                    return LOHThreshold - Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);
                }
            }

            public bool CanTransformMultipleBlocks
            {
                get
                {
                    return true;
                }
            }

            public bool CanReuseTransform
            {
                get
                {
                    return false;
                }
            }

            internal SymmetricAlgorithmDecryptTransform(SymmetricAlgorithm algorithm)
            {
                Algorithm = algorithm;
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (inputCount == 0) return 0;

                if (inputCount % InputBlockSize != 0) {
                    throw new ArgumentException($"Input count must be a multiple of {InputBlockSize}.");
                }

                int bytesWritten = 0;

                while (inputCount > 0)
                {
                    int count = Algorithm.DecryptBytes(new ArraySegment<byte>(inputBuffer, inputOffset + bytesWritten, InputBlockSize), outputBuffer, outputOffset);

                    bytesWritten += count;
                    inputCount -= count;
                }

                return bytesWritten;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputCount > InputBlockSize) {
                    throw new ArgumentException($"Input count must be less than or equal to {InputBlockSize}.");
                }

                byte[] output = new byte[OutputBlockSize];
                int bytesWritten = Algorithm.DecryptBytes(new ArraySegment<byte>(inputBuffer, inputOffset, inputCount), output, 0);
            
                if (bytesWritten != output.Length) {
                    Array.Resize(ref output, bytesWritten);
                }

                return output;
            }

            public void Dispose()
            {
                Algorithm = null;
            }
        }
    }
}