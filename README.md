# DataProto
A C# binary serialisation/deserialisation library aimed at being compatible with the nodeJS equivalent,
[dataproto](https://www.npmjs.com/package/dataproto?activeTab=readme). Quite similar to BinaryReader/BinaryWriter
inbuilt C# classes, except with QOL improvements such as support for variable length integer and string types.
Intended for use in high performance networking scenarios, such as games, where low bandwidth usage, speed and
efficient data transmission is needed.

## DataReader, DataWriter
Read from and write to a data buffer, with static typing

```csharp
    using DataProto;
    using System.Diagnostics;
    
    var writer = new WriteablePacket();
    
    writer.WriteInt(123);
    writer.WriteString("Hello world!");
    
    var buffer = writer.ToArray(); // byte[17] { 0, 0, 0, 123, 12, ... }
    
    var reader = new ReadablePacket(buffer);
    
    Debug.Assert(reader.ReadInt() == 123);
    Debug.Assert(reader.ReadString() == "Hello World!");
```

## Schemas
This is meant for situations where the type to be written cannot be known ahead of time, such as in reflective
or dynamic scenarios. For performance reasons, don't use this unless you have to.

```csharp
    using DataProto;
    using System.IO;
    using System.Text.Json;
    
    var schema = new
    {
        A = "Hello",
        B = new int[3]
    };

    var writer = new WriteablePacket();
    
    writer.Write(schema);
    File.WriteAllBytes("./SomeObject.dat", writer);
    
    var reader = new ReadablePacket(File.ReadAllBytes("./SomeObject.dat"));
    Console.WriteLine(JsonSerializer.Serialize(reader.Read(schema)));
```

# TODO:
See [issues](https://github.com/Zekiah-A/DataProto/issues)
