namespace lab1.Core.Providers;

public sealed record WorkerSnapshot(int? OrderNumber, string? CustomerName, string? CustomerSurname)
{
    public bool IsIdle => OrderNumber is null;

    public bool IsBusy => !IsIdle;
}