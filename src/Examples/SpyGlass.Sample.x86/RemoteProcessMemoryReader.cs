using System;
using AsmResolver;

namespace SpyGlass.Sample.x86
{
    // TODO: do buffering for speedup?
    
    internal class RemoteProcessMemoryReader : IBinaryStreamReader
    {
        private readonly RemoteProcess _process;

        public RemoteProcessMemoryReader(RemoteProcess process, IntPtr start)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            StartPosition = Position = (long) start;
        }

        public long StartPosition
        {
            get;
        }

        public long Position
        {
            get;
            set;
        }

        public long Length
        {
            get;
        }
        
        public IBinaryStreamReader CreateSubReader(long address, int size)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytesUntil(byte value)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            _process.ReadMemory((IntPtr) Position, buffer, 0, buffer.Length);
            Position += count;
            return buffer;
        }

        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadBytes(2), 0);
        }

        public uint ReadUInt32()
        {
            return BitConverter.ToUInt16(ReadBytes(4), 0);
        }

        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt16(ReadBytes(8), 0);
        }

        public sbyte ReadSByte()
        {
            return unchecked((sbyte) ReadByte());
        }

        public short ReadInt16()
        {
            return unchecked((short) ReadUInt16());
        }

        public int ReadInt32()
        {
            return unchecked((int) ReadUInt32());
        }

        public long ReadInt64()
        {
            return unchecked((long) ReadUInt64());
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(8), 0);
        }
    }
}