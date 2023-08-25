using DataProto;

// Schema
Console.WriteLine("Schema test");
var buyer = new
{
    Name = "Joe",
    Age = 17,
    Bought = new[]
    {
        Products.IceCream with { QuantityBought = 4 },
        Products.Beans with { QuantityBought = 2 },
        Products.Peas
    },
    Details = new BillingDetails
    (
        CardNumberParser.Parse("1234-5678-91011-1213"),
        "Joe Joeson Mama",
        420
    )
};

try
{
    Console.WriteLine("-----------");
    Console.WriteLine(buyer);

    var writer = new WriteablePacket();
    writer.Write(buyer);

    Console.WriteLine("-----------");
    Console.Write("Output data: ");
    foreach (var @byte in writer.ToArray())
    {
        Console.Write(@byte.ToString());
    }
    Console.WriteLine("");
    
    Console.WriteLine("-----------");
    Console.WriteLine("Read asserts:");
    
    var reader = new ReadablePacket(writer);
    var name = reader.ReadString();
    Console.WriteLine("Name: {0}, Valid: {1}", name, name == buyer.Name);

    var age = reader.ReadInt();
    Console.WriteLine("Age: {0}, Valid: {1}", age, age == buyer.Age);

    // TODO: Make it so that we no longer need a target when reading complex objects.
    // TODO: Target is currently used to hold a default value so that the read function is able to output
    // TODO: what it has read into a template of the object, and with array types, it uses the target length to
    // TODO: determine how many elements of the array need to be read. This could be fixed by replacing target
    // TODO: with an array length argument for attays, or instead mandating a special function like "ReadObjectArray", The other
    // TODO: issues could be fixed by just allowing dataproto to only take in type, and then be able to create the instance of
    // TODO: the object it will output what it has read to, such as byh using Activator.CreateInstance() on the given type
    var bought = reader.Read(new[] { Products.None, Products.None, Products.None }, typeof(Product[]));
    Console.WriteLine("Bought: {0}, Valid: {1}", bought, bought == buyer.Bought);

    var details = reader.Read(new BillingDetails(0, string.Empty, 0), typeof(BillingDetails));
    Console.WriteLine(details);
}
catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static class CardNumberParser
{
    public static long Parse(string number)
    {
        number = number.Replace("-", string.Empty);
        return long.Parse(number);
    }
}

static class Products
{
    public static Product None = new(string.Empty, 0.0f);
    public static Product IceCream = new("Ice cream", 1.50f);
    public static Product Peas = new("Peas", 2.00f);
    public static Product Beans = new("Beans", 1.25f);
}

record Product(string Name, float Price)
{
    public int QuantityBought { get; set; } = 1;
}

record BillingDetails(long Number, string FullName, int Cve);