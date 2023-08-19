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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string data)
    {
        var size = Encoding.UTF8.GetByteCount(data);
        var bytes = Encoding.UTF8.GetBytes(data);

        EnsureCapacity(size);

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