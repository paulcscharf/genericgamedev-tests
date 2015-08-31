using System;
using System.IO;
using System.Runtime.InteropServices;

namespace genericgamedev_tests.StructByteArrayConversion
{
    [Serializable]
    struct Struct16Bytes : ISerialisable
    {
        public int AnInteger;
        public float AFloat;
        public long ALong;

        public static Struct16Bytes Filled()
        {
            return new Struct16Bytes
            {
                AnInteger = 1337,
                AFloat = 3.14f,
                ALong = 13374242427331
            };
        }

        private static readonly int size = Marshal.SizeOf(typeof (Struct16Bytes));
        private static readonly MemoryStream stream = new MemoryStream(size);
        private static readonly BinaryWriter writer = new BinaryWriter(stream);
        private static readonly BinaryReader reader = new BinaryReader(stream);

        public byte[] ToArrayWithBinaryWriter()
        {
            //var stream = new MemoryStream(16);
            //var writer = new BinaryWriter(stream);

            stream.Position = 0;

            writer.Write(this.AnInteger);
            writer.Write(this.AFloat);
            writer.Write(this.ALong);

            return stream.ToArray();
        }

        public void FromArrayWithBinaryReader(byte[] bytes)
        {
            //var reader = new BinaryReader(new MemoryStream(bytes));

            stream.Position = 0;
            stream.Write(bytes, 0, size);
            stream.Position = 0;

            this.AnInteger = reader.ReadInt32();
            this.AFloat = reader.ReadSingle();
            this.ALong = reader.ReadInt64();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", this.AnInteger, this.AFloat, this.ALong);
        }

        public bool Equals(Struct16Bytes other)
        {
            return this.AnInteger == other.AnInteger && this.AFloat == other.AFloat && this.ALong == other.ALong;
        }
    }

    interface ISerialisable
    {
        byte[] ToArrayWithBinaryWriter();
        void FromArrayWithBinaryReader(byte[] bytes);
    }
}