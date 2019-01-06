using System;

using Kirkin.Collections.Generic;
using Kirkin.Security.Cryptography.Internal;

namespace Kirkin.Security.Cryptography
{
    partial class SymmetricAlgorithm
    {
        sealed class SymmetricAlgorithmEncryptTransform : ChunkedTransform
        {
            public SymmetricAlgorithm Algorithm { get; private set; }

            public override int InputBlockSize
            {
                get
                {
                    // MaxEncryptOutputBufferSize of an empty array represents pure overhead.
                    int algorithmOverhead = Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                    return (ChunkSize - algorithmOverhead) / Aes256Cbc.BlockSizeInBytes * Aes256Cbc.BlockSizeInBytes;
                }
            }

            public override int OutputBlockSize
            {
                get
                {
                    int algorithmOverhead = Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                    return InputBlockSize + algorithmOverhead;
                }
            }

            internal SymmetricAlgorithmEncryptTransform(SymmetricAlgorithm algorithm)
            {
                Algorithm = algorithm;
            }

            protected override int TransformChunk(in ArraySegment<byte> chunk, byte[] outputBuffer, int outputOffset)
            {
                return Algorithm.EncryptBytes(chunk, outputBuffer, outputOffset);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                Algorithm = null;
            }
        }

        sealed class SymmetricAlgorithmDecryptTransform : ChunkedTransform
        {
            public SymmetricAlgorithm Algorithm { get; private set; }

            public override int InputBlockSize
            {
                get
                {
                    int algorithmOverhead = Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                    return OutputBlockSize + algorithmOverhead;
                }
            }

            public override int OutputBlockSize
            {
                get
                {
                    // MaxEncryptOutputBufferSize of an empty array represents pure overhead.
                    int algorithmOverhead = Algorithm.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                    return (ChunkSize - algorithmOverhead) / Aes256Cbc.BlockSizeInBytes * Aes256Cbc.BlockSizeInBytes;
                }
            }

            internal SymmetricAlgorithmDecryptTransform(SymmetricAlgorithm algorithm)
            {
                Algorithm = algorithm;
            }

            protected override int TransformChunk(in ArraySegment<byte> chunk, byte[] outputBuffer, int outputOffset)
            {
                return Algorithm.DecryptBytes(chunk, outputBuffer, outputOffset);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                Algorithm = null;
            }
        }
    }
}