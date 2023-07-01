using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataProto;

/// <summary>
/// Packet reader, most similar to client/data.js
/// </summary>
public ref struct ReadablePacket
{
    public Span<byte> Data;
    public int Position;
    public int Left => Data.Length - Position;

    public ReadablePacket(byte[] data)
    {
        Data = data;
    }

    public object? Read(object? target, Type type)
    {
        if (type == typeof(byte) || type == typeof(sbyte)) return ReadByte();
        if (type == typeof(short)) return ReadShort();
        if (type == typeof(ushort)) return ReadUShort();
        if (type == typeof(int)) return ReadInt();
        if (type == typeof(uint)) return ReadUInt();
        if (type == typeof(double)) return ReadDouble();
        if (type == typeof(float)) return ReadFloat();
        if (type == typeof(bool)) return ReadBool();
        if (type == typeof(string)) return ReadString();
        if (type == typeof(byte[])) return ReadByteArray();
        
        // If it is a decodable, such an an item, then the item's deserializer will handle this data
        if (typeof(IDecodable).IsAssignableFrom(type))
        {
            var decodeInfo = type.GetMethod(nameof(IDecodable.Decode));
            return decodeInfo?.CreateDelegate<IDecodable.DecodeDelegate>(target)(ref this);
        }

        // If it is an array, (but not a byte[]), then we hope that the target is an instance so we can extract it's
        //  length, and populate the array with data of the correct type.
        if (target is object[] arrayTarget)
        {
            for (var i = 0; i < arrayTarget.Length; i++)
            {
                arrayTarget[i] = Read(arrayTarget[i], type.GetElementType()!)!;
            }
        }

        // Otherwise, we will recurse through their properties and decode each, this is a recursive method, so 
        foreach (var property in type.GetProperties())
        {
            var propertyValue = Read(property.GetValue(target), property.PropertyType);
            target?.GetType().GetProperty(property.Name)!.SetValue(target, propertyValue);
        }

        return target;
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte() => Data[Position++];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Data[((Position += 2) - 2)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUShort() => BinaryPrimitives.ReadUInt16BigEndian(Data[((Position += 2) - 2)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Data[((Position += 4) - 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt() => BinaryPrimitives.ReadUInt32BigEndian(Data[((Position += 4) - 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble() => BinaryPrimitives.ReadDoubleBigEndian(Data[((Position += 8) - 8)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat() => BinaryPrimitives.ReadSingleBigEndian(Data[((Position += 4) - 4)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool() => Data[Position++] != 0;

    /// <summary>
    /// A variable length integer. Similar to VarInt, made up of UInt6, UInt14 or Uint31, allows range 0-2147483647.
    /// </summary>
    public uint ReadFlexInt()
    {
        var value = (uint) Data[Position];
        if (value >= 64)
        {
            if (value >= 128)
            {
                value = BinaryPrimitives.ReadUInt32BigEndian(Data[Position..]) & 0x7FFFFFFF;
                Position += 4;
            }
            else
            {
                value = (uint) (BinaryPrimitives.ReadUInt16BigEndian(Data[Position..]) & 0x3FFF);
                Position += 2;
            }
        }
        else
        {
            Position++;
        }

        return value;
    }
    
    public byte[] ReadBytes(int count)
    {
        var array = Data[Position..(Position + count)];
        Position += count;
        return array.ToArray();
    }

    /// <summary>
    /// Variable length byte array, the first value will represent a FlexInt of the array length.
    /// </summary>
    public byte[] ReadByteArray()
    {
        var length = ReadFlexInt();
        var array = Data[Position..(int) (Position + length)].ToArray();
        Position += (int) length;
        return array;
    }

    /// <summary>
    /// Variable length string, the first value will represent a FlexInt of the array length.
    /// </summary>
    public string ReadString()
    {
        var subArray = ReadByteArray();
        return Encoding.UTF8.GetString(subArray);
    }
    
    public static implicit operator ReadablePacket(byte[] data)
    {
        return new ReadablePacket(data);
    }

    public static implicit operator byte[](ReadablePacket packet)
    {
        return packet.Data.ToArray();
    }
    
    public static implicit operator Span<byte>(ReadablePacket packet)
    {
        return packet.Data;
    }

    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }
}