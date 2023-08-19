using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataProto;

/// <summary>
/// Packet builder, most similar to client/data.js
/// </summary>
public ref struct WriteablePacket
{
    public Span<byte> Data { get; private set; }
    public int Position { get; private set; }

    public WriteablePacket(byte[]? data = null, int defaultSize = 2)
    {
        Data = data ?? new byte[defaultSize];
    }

    public void Write(object instance)
    {
        foreach (var property in instance.GetType().GetProperties())
        {
            var value = property.GetValue(instance);

            if (value is null)
            {
                continue;
            }

            if (value is object[] arrayValue)
            {
                foreach(var item in arrayValue)
                {
                    Write(item);
                }

                continue;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte or TypeCode.SByte or TypeCode.Boolean:
                    WriteByte(Convert.ToByte(value));
                    break;
                // case TypeCode.Char:
                //     break;
                case TypeCode.Int16:
                    WriteShort((short) value);
                    break;
                case TypeCode.Object:
                    Write(value);
                    break;
                case TypeCode.UInt16:
                    WriteUShort((ushort) value);
                    break;
                case TypeCode.Int32:
                    WriteInt((int) value);
                    break;
                case TypeCode.UInt32:
                    WriteUInt((uint) value);
                    break;
                case TypeCode.Int64:
                    WriteLong((long) value);
                    break;
                 case TypeCode.UInt64:
                    WriteULong((ulong) value);
                    break;
                case TypeCode.Single:
                    WriteFloat((float) value);
                    break;
                case TypeCode.Double:
                    WriteDouble((double) value);
                    break;
                // case TypeCode.Decimal:
                //     break;
                case TypeCode.String:
                    WriteString((string) value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte data)
    {
        EnsureCapacity(sizeof(byte));

        Data[Position] = data;
        Position++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool data)
    {
        EnsureCapacity(sizeof(bool));

        Data[Position] = (byte) (data ? 1 : 0);
        Position++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUShort(ushort data)
    {
        EnsureCapacity(sizeof(ushort));

        BinaryPrimitives.WriteUInt16BigEndian(Data[Position..], data);
        Position += sizeof(ushort);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteShort(short data)
    {
        EnsureCapacity(sizeof(short));

        BinaryPrimitives.WriteInt16BigEndian(Data[Position..], data);
        Position += sizeof(short);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt(uint data)
    {
        EnsureCapacity(sizeof(uint));

        BinaryPrimitives.WriteUInt32BigEndian(Data[Position..], data);
        Position += sizeof(uint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt(int data)
    {
        EnsureCapacity(sizeof(int));

        BinaryPrimitives.WriteInt32BigEndian(Data[Position..], data);
        Position += sizeof(int);
    }

    public void WriteLong(long data)
    {
        EnsureCapacity(sizeof(long));

        BinaryPrimitives.WriteInt64BigEndian(Data[Position..], data);
        Position += sizeof(long);
    }

    public void WriteULong(ulong data)
    {
        EnsureCapacity(sizeof(ulong));

        BinaryPrimitives.WriteUInt64BigEndian(Data[Position..], data);
        Position += sizeof(ulong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float data)
    {
        EnsureCapacity(sizeof(float));

        BinaryPrimitives.WriteSingleBigEndian(Data[Position..], data);
        Position += sizeof(float);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double data)
    {
        EnsureCapacity(sizeof(double));

        BinaryPrimitives.WriteDoubleBigEndian(Data[Position..], data);
        Position += sizeof(double);
    }

    public void WriteString(string data)
    {
        var stringBytes = Encoding.UTF8.GetBytes(data);
        var stringSize = stringBytes.Length;
        var i = 0;

        if (stringSize > 0x3FFF)
        {
            if (stringSize > 0x7FFFFFFF)
            {
                throw new ArgumentOutOfRangeException("Encoded strings may not have more than 2147483647 characters");
            }

            WriteUInt((uint) stringSize);
        }
        else if (stringSize > 0x3F)
        {
            WriteUShort((ushort) stringSize);
        }
        else
        {
            WriteByte((byte) stringSize);
        }

        EnsureCapacity(stringSize);

        for (;i < data.Length; i++)
        {
            Data[Position + i] = stringBytes[i];
        }
        Position += stringSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray()
    {
        return Data[..Position].ToArray();
    }

    public static implicit operator WriteablePacket(byte[] data)
    {
        return new WriteablePacket(data);
    }

    public static implicit operator byte[](WriteablePacket packet)
    {
        return packet.Data[..packet.Position].ToArray();
    }

    public static implicit operator Span<byte>(WriteablePacket packet)
    {
        return packet.Data[..packet.Position];
    }

    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }

    private void EnsureCapacity(int size)
    {
        if (Data.Length > Position + size)
        {
            return;
        }

        var buffer = new byte[Data.Length + size];
        Data.CopyTo(buffer);
        Data = buffer;
    }
}