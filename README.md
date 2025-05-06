# OPC UA Chaos Server

A malicious OPC UA server designed to simulate unstable server conditions for testing OPC UA clients. Implemented in .NET 8, this project allows fine-grained chaos injection scenarios via configuration or environment variables.

## 🚀 Features

* Dynamic chaos injection modes
* Realistic OPC UA server behavior
* Self-signed certificate creation and validation
* Structured logging via Microsoft.Extensions.Logging and Serilog
* Dockerized for easy deployment

## 🔧 Chaos Modes

Supported `CHAOS_MODE` values:

| Mode                  | Description                                          |
| --------------------- | ---------------------------------------------------- |
| `default`             | Clears monitored items **and** disables publishing   |
| `clear_items`         | Deletes all monitored items in subscriptions         |
| `break_engine`        | Disables publishing for active subscriptions         |
| `remove_subscription` | Completely deletes the subscription from the session |
| `close_session`       | Closes and removes the client session                |
| `none`                | No chaos (default if unset)                          |

Chaos is injected every `CHAOS_INTERVAL` seconds with a probability `CHAOS_PROBABILITY` (0.0 to 1.0).

## 📁 Directory Structure

```
.
├── Program.cs                    # Host setup with Serilog and DI
├── ChaosServer.cs               # Core OPC UA server with chaos logic
├── ChaosServerApplication.cs    # Application lifecycle and certificate config
├── ChaosNodeManager.cs          # Node structure and namespace
├── ChaosOptions.cs              # Typed options via env/config
├── ChaosMode.cs                 # Enum definition for chaos modes
├── ReflectionHelper.cs          # Internal accessor helper for private session lists
├── Dockerfile                   # Docker container definition
```

## ⚙️ .NET Configuration Binding

The server uses `IOptions<ChaosOptions>` and binds config from environment variables or config providers like `appsettings.json`, secrets or CLI args.

```csharp
public class ChaosOptions
{
    public ChaosMode Mode { get; set; } = ChaosMode.CloseSession;
    public int IntervalSeconds { get; set; } = 5;
    public double Probability { get; set; } = 0.4;
    public int StaticItems { get; set; } = 2;
    public int DynamicItems { get; set; } = 3;
}
```

Environment variable examples:

* `CHAOS_MODE=RemoveSubscription`
* `CHAOS_INTERVAL=10`
* `CHAOS_PROBABILITY=0.25`
* `CHAOS_STATICITEMS=3`
* `CHAOS_DYNAMICITEMS=5`

## 🏑 Environment Variables

| Variable             | Default | Description                              |
| -------------------- | ------- | ---------------------------------------- |
| `CHAOS_MODE`         | `none`  | Chaos injection mode                     |
| `CHAOS_INTERVAL`     | `5`     | Interval in seconds between injections   |
| `CHAOS_PROBABILITY`  | `0.5`   | Chance (0.0 to 1.0) to trigger an action |
| `CHAOS_STATICITEMS`  | `2`     | Number of static variables to expose     |
| `CHAOS_DYNAMICITEMS` | `3`     | Number of dynamic (changing) variables   |

## 📦 Docker Usage

```bash
docker run -p 4840:4840 \
  -e CHAOS_MODE=remove_subscription \
  -e CHAOS_INTERVAL=3 \
  -e CHAOS_PROBABILITY=0.7 \
  ghcr.io/BoBiene/opcua-chaos-server
```

## 🔗 Certificate Handling

If no application certificate is found, one is auto-generated and stored under `./pki/own`. The server uses directory-based trust lists for simplicity.

## ✅ Use Cases

* Reproduce real-world OPC UA client errors (e.g. `BadSubscriptionIdInvalid`)
* Test subscription resilience
* Evaluate retry logic and fault recovery

## 🚫 Disclaimer

This server is intended **only for testing purposes** in controlled environments. Do not expose it to production networks.

---

Maintained by [@BoBiene](https://github.com/BoBiene)
