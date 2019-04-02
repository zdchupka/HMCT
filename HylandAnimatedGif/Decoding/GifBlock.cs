using System;
using System.Collections.Generic;
using System.IO;

namespace WpfAnimatedGif.Decoding
{
    internal enum GifBlockKind
    {
        Control,
        GraphicRendering,
        SpecialPurpose,
        Other
    }

    internal struct GifColor
    {
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;

        internal GifColor(byte r, byte g, byte b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public byte R { get { return _r; } }
        public byte G { get { return _g; } }
        public byte B { get { return _b; } }

        public override string ToString()
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", _r, _g, _b);
        }
    }

    [Serializable]
    internal class GifDecoderException : Exception
    {
        internal GifDecoderException() { }
        internal GifDecoderException(string message) : base(message) { }
        internal GifDecoderException(string message, Exception inner) : base(message, inner) { }
        protected GifDecoderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    internal abstract class GifBlock
    {
        internal static GifBlock ReadBlock(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            int blockId = stream.ReadByte();
            if (blockId < 0)
                throw GifHelpers.UnexpectedEndOfStreamException();
            switch (blockId)
            {
                case GifExtension.ExtensionIntroducer:
                    return GifExtension.ReadExtension(stream, controlExtensions, metadataOnly);
                case GifFrame.ImageSeparator:
                    return GifFrame.ReadFrame(stream, controlExtensions, metadataOnly);
                case GifTrailer.TrailerByte:
                    return GifTrailer.ReadTrailer();
                default:
                    throw GifHelpers.UnknownBlockTypeException(blockId);
            }
        }

        internal abstract GifBlockKind Kind { get; }
    }
}

