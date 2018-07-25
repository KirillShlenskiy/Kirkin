using System.Xml.Serialization;

namespace Kirkin.Media.FFprobe
{
    [XmlType("stream")]
    public sealed class FFprobeStreamInfo
    {
        [XmlAttribute("codec_type")]
        public string CodecType { get; set; }

        [XmlAttribute("codec_name")]
        public string CodecName { get; set; }

        [XmlAttribute("codec_tag_string")]
        public string CodecTagString { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("duration")]
        public decimal Duration { get; set; }

        [XmlAttribute("bit_rate")]
        public int BitRate { get; set; }
    }
}