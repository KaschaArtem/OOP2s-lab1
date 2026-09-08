using lab1.Core.Models;

namespace lab1.Core.Utils;

public class CustomerFactory
{
    private readonly string[] Names =
    {
        "Adam", "Ben", "Carla", "Dana", "Ethan",
        "Fiona", "George", "Hannah", "Ivan", "Julia",
    };

    private readonly string[] Surnames =
    {
        "Anderson", "Brown", "Clarke", "Dawson", "Evans",
        "Foster", "Grant", "Hyde", "Irwin", "Jensen",
    };

    private readonly Random _random = new();

    public Customer Create()
    {
        var name = Names[_random.Next(Names.Length)];
        var surname = Surnames[_random.Next(Surnames.Length)];
        return new Customer(name, surname);
    }
}
