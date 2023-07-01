using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataProto;

/// <summary>
/// Packet builder, most similar to client/data.js
/// </summary>
public ref struct WriteablePacket
{
    public byte[] RawData;
    public Span<byte> Data;
    public int Position = 0;
    public int ReallocSize = 512;

    public WriteablePacket(byte[]? data = null)
    {
        RawData = data ?? new byte[ReallocSize];
        Data = new Span<byte>(RawData);
    }

    private void Realloc()
    {
        Array.Resize(ref RawData, RawData.Length + ReallocSize);
        Data = new Span<byte>(RawData);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte data)
    {
        if (Position == Data.Length)
        {
            Realloc();
        }

        Data[Position] = data;
        Position++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool data)
    {
        if (Position == Data.Length)
        {
            Realloc();
        }

        Data[Position] = (byte) (data ? 1 : 0);
        Position++;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUShort(ushort data)
    {
        while (Position + sizeof(ushort) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteUInt16BigEndian(Data[Position..], data);
        Position += sizeof(ushort);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteShort(short data)
    {
        while (Position + sizeof(short) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteInt16BigEndian(Data[Position..], data);
        Position += sizeof(short);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt(uint data)
    {
        while (Position + sizeof(uint) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteUInt32BigEndian(Data[Position..], data);
        Position += sizeof(uint);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt(int data)
    {
        while (Position + sizeof(int) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteInt32BigEndian(Data[Position..], data);
        Position += sizeof(int);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float data)
    {
        while (Position + sizeof(float) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteSingleBigEndian(Data[Position..], data);
        Position += sizeof(float);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double data)
    {
        while (Position + sizeof(double) >= Data.Length)
        {
            Realloc();
        }

        BinaryPrimitives.WriteDoubleBigEndian(Data[Position..], data);
        Position += sizeof(double);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string data)
    {
        var size = Encoding.UTF8.GetByteCount(data);
        var bytes = Encoding.UTF8.GetBytes(data);
        while (Position + size >= Data.Length)
        {
            Realloc();
        }

        if (size > 16383)
        {
            if (size > 2147483647)
            {
                
            }
        }

        for (var i = 0; i < data.Length; i++)
        {
            Data[Position + i] = bytes[i];
        }
        Position += size;
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
}