using System;

using Kirkin.Collections.Generic;

namespace Kirkin.Security.Cryptography.Internal
{
    internal sealed class SymmetricDecryptTransform : ChunkedTransform
    {
        public SymmetricCryptoFormatter Formatter { get; private set; }

        public override int InputBlockSize
        {
            get
            {
                int formatOverhead = Formatter.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                return OutputBlockSize + formatOverhead;
            }
        }

        public override int OutputBlockSize
        {
            get
            {
                // MaxEncryptOutputBufferSize of an empty array represents pure overhead.
                int formatOverhead = Formatter.MaxEncryptOutputBufferSize(Array<byte>.Empty);

                return (ChunkSize - formatOverhead) / Formatter.BlockSize * Formatter.BlockSize;
            }
        }

        internal SymmetricDecryptTransform(SymmetricCryptoFormatter formatter)
        {
            Formatter = formatter;
        }

        protected override int TransformChunk(in ArraySegment<byte> chunk, byte[] outputBuffer, int outputOffset)
        {
            return Formatter.DecryptBytes(chunk, outputBuffer, outputOffset);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Formatter = null;
        }
    }
}