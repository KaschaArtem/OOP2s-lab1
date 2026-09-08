namespace lab1.Core.Utils;

public sealed class OrderNumberGenerator
{
    private int _next;

    public int Next() => Interlocked.Increment(ref _next);
}
