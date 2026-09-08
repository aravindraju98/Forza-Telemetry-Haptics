using System.Net;
using System.Net.Sockets;

namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Background UDP listener for FH6 Data Out. Recovers from errors and never
/// lets a malformed packet kill the process.
/// </summary>
public sealed class TelemetryReceiver : IDisposable
{
    private readonly ITelemetryParser _parser;
    private readonly TelemetryNormalizer _normalizer;
    private readonly object _gate = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private UdpClient? _udp;

    private VehicleTelemetry _latest = VehicleTelemetry.Empty;
    private DateTime? _lastPacketUtc;
    private long _accepted;
    private long _malformed;
    private string _lastError = string.Empty;
    private string _bindEndpoint = string.Empty;
    private readonly Queue<DateTime> _recentPackets = new();

    public TelemetryReceiver(ITelemetryParser parser, TelemetryNormalizer normalizer)
    {
        _parser = parser;
        _normalizer = normalizer;
    }

    public void Start(string bindAddress, int port)
    {
        Stop();
        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => ListenLoop(bindAddress, port, _cts.Token));
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _udp?.Close();
        }
        catch
        {
            // ignored
        }

        try
        {
            _loop?.Wait(TimeSpan.FromMilliseconds(400));
        }
        catch
        {
            // ignored
        }

        _cts?.Dispose();
        _cts = null;
        _loop = null;
        _udp = null;
    }

    public TelemetrySnapshot Snapshot(TimeSpan staleAfter)
    {
        lock (_gate)
        {
            var now = DateTime.UtcNow;
            while (_recentPackets.Count > 0 && now - _recentPackets.Peek() > TimeSpan.FromSeconds(1))
            {
                _recentPackets.Dequeue();
            }

            var rate = _recentPackets.Count;
            TelemetryLinkState state;
            if (_lastPacketUtc is null)
            {
                state = TelemetryLinkState.Waiting;
            }
            else if (now - _lastPacketUtc.Value > staleAfter)
            {
                state = TelemetryLinkState.Stale;
            }
            else
            {
                state = TelemetryLinkState.Live;
            }

            return new TelemetrySnapshot
            {
                State = state,
                Telemetry = _latest,
                LastPacketUtc = _lastPacketUtc,
                PacketRateHz = rate,
                AcceptedPackets = _accepted,
                MalformedPackets = _malformed,
                LastError = _lastError,
                BindEndpoint = _bindEndpoint
            };
        }
    }

    private void ListenLoop(string bindAddress, int port, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!IPAddress.TryParse(bindAddress, out var address))
                {
                    address = IPAddress.Any;
                }

                var endpoint = new IPEndPoint(address, port);
                _udp = new UdpClient(endpoint);
                _udp.Client.ReceiveTimeout = 250;
                lock (_gate)
                {
                    _bindEndpoint = endpoint.ToString();
                    _lastError = string.Empty;
                }

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var remote = new IPEndPoint(IPAddress.Any, 0);
                        var data = _udp.Receive(ref remote);
                        var now = DateTime.UtcNow;
                        if (!_parser.TryParse(data, out var packet, out var error) || packet is null)
                        {
                            lock (_gate)
                            {
                                _malformed++;
                                _lastError = error ?? "Malformed packet.";
                            }

                            continue;
                        }

                        var normalized = _normalizer.Normalize(packet, now);
                        lock (_gate)
                        {
                            _latest = normalized;
                            _lastPacketUtc = now;
                            _accepted++;
                            _recentPackets.Enqueue(now);
                            _lastError = string.Empty;
                        }
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode is SocketError.TimedOut or SocketError.Interrupted)
                    {
                        // idle poll so cancellation can be observed
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                lock (_gate)
                {
                    _lastError = $"Listener restarting: {ex.Message}";
                }

                try
                {
                    Task.Delay(400, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            finally
            {
                try
                {
                    _udp?.Dispose();
                }
                catch
                {
                    // ignored
                }

                _udp = null;
            }
        }
    }

    public void Dispose() => Stop();
}
