using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lab1.Core;
using lab1.Core.Models;
using lab1.Core.Providers;

namespace lab1.UI.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(100);
    private const double MaxSeconds = 3600;

    [ObservableProperty]
    private string _arrivalSecondsText = "2";

    [ObservableProperty]
    private string _orderingSecondsText = "2.5";

    [ObservableProperty]
    private string _prepSecondsText = "3";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isRunning;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _orderLineCount;

    [ObservableProperty]
    private IReadOnlyList<Customer> _orderLineCustomers = Array.Empty<Customer>();

    [ObservableProperty]
    private int? _orderTakerOrderNumber;

    [ObservableProperty]
    private string? _orderTakerCustomerName;

    [ObservableProperty]
    private int? _kitchenCurrentOrderNumber;

    [ObservableProperty]
    private string? _kitchenCurrentCustomerName;

    [ObservableProperty]
    private IReadOnlyList<Order> _kitchenWaitingOrders = Array.Empty<Order>();

    [ObservableProperty]
    private int _kitchenWaitingCount;

    [ObservableProperty]
    private IReadOnlyList<Customer> _pickupWaitingCustomers = Array.Empty<Customer>();

    [ObservableProperty]
    private int _servingLineCount;

    private FastFoodSimulator? _simulator;
    private SimulationDataProvider? _provider;
    private CancellationTokenSource? _cts;
    private Task? _runTask;
    private Task? _streamTask;

    public bool IsNotRunning => !IsRunning;

    private bool CanStart() => !IsRunning;

    private bool CanStop() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        HasError = false;
        ValidationMessage = string.Empty;

        if (!TryParseSeconds(ArrivalSecondsText, "customer arrival", out var arrival) ||
            !TryParseSeconds(OrderingSecondsText, "order taking", out var ordering) ||
            !TryParseSeconds(PrepSecondsText, "cook preparation", out var prep))
        {
            return;
        }

        var cts = new CancellationTokenSource();
        var simulator = new FastFoodSimulator(
            TimeSpan.FromSeconds(arrival),
            TimeSpan.FromSeconds(ordering),
            TimeSpan.FromSeconds(prep));
        var provider = new SimulationDataProvider(simulator);
        provider.Updated += OnUpdated;

        var token = cts.Token;
        _cts = cts;
        _simulator = simulator;
        _provider = provider;

        _runTask = Task.Run(() => simulator.RunAsync(token), token);
        _streamTask = Task.Run(() => provider.StreamUpdatesAsync(RefreshInterval, token), token);

        IsRunning = true;
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        if (_provider is not null)
            _provider.Updated -= OnUpdated;

        _cts?.Cancel();

        IsRunning = false;
        ClearSnapshot();
    }

    private bool TryParseSeconds(string text, string label, out double seconds)
    {
        seconds = 0;
        var trimmed = (text ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            Fail($"The {label} interval is required.");
            return false;
        }

        var parsed =
            double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) ||
            double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out seconds);

        if (!parsed || double.IsNaN(seconds) || double.IsInfinity(seconds))
        {
            Fail($"Enter a valid number of seconds for the {label} interval.");
            return false;
        }

        if (seconds <= 0)
        {
            Fail($"The {label} interval must be greater than zero.");
            return false;
        }

        if (seconds > MaxSeconds)
        {
            Fail($"The {label} interval must be {MaxSeconds.ToString(CultureInfo.InvariantCulture)} seconds or less.");
            return false;
        }

        return true;
    }

    private void Fail(string message)
    {
        ValidationMessage = message;
        HasError = true;
    }

    private void OnUpdated(object? sender, SimulationSnapshot snapshot)
    {
        if (!IsRunning)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (IsRunning)
                ApplySnapshot(snapshot);
        });
    }

    private void ApplySnapshot(SimulationSnapshot s)
    {
        OrderLineCount = s.OrderLineCount;
        OrderLineCustomers = s.OrderLineCustomers;
        OrderTakerOrderNumber = s.OrderTakerOrderNumber;
        OrderTakerCustomerName = s.OrderTakerCustomerName;
        KitchenCurrentOrderNumber = s.KitchenCurrentOrderNumber;
        KitchenCurrentCustomerName = s.KitchenCurrentCustomerName;
        KitchenWaitingOrders = s.KitchenWaitingOrders;
        KitchenWaitingCount = s.KitchenWaitingCount;
        PickupWaitingCustomers = s.PickupWaitingCustomers;
        ServingLineCount = s.ServingLineCount;
    }

    private void ClearSnapshot()
    {
        OrderLineCount = 0;
        OrderLineCustomers = Array.Empty<Customer>();
        OrderTakerOrderNumber = null;
        OrderTakerCustomerName = null;
        KitchenCurrentOrderNumber = null;
        KitchenCurrentCustomerName = null;
        KitchenWaitingOrders = Array.Empty<Order>();
        KitchenWaitingCount = 0;
        PickupWaitingCustomers = Array.Empty<Customer>();
        ServingLineCount = 0;
    }

    public void Dispose()
    {
        Stop();

        try
        {
            _runTask?.Wait(TimeSpan.FromSeconds(2));
            _streamTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }

        _cts?.Dispose();
        _cts = null;
        _provider = null;
        _simulator = null;
        GC.SuppressFinalize(this);
    }
}
