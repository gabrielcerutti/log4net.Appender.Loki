# Log4Net Grafana Loki Appender

[![Build Status](https://github.com/gabrielcerutti/log4net.Appender.Loki/actions/workflows/netframework.build.yml/badge.svg)](https://github.com/gabrielcerutti/log4net.Appender.Loki/actions/workflows/netframework.build.yml)
[![NuGet](https://img.shields.io/nuget/v/Log4Net.Appender.Grafana.Loki.svg)](https://www.nuget.org/packages/Log4Net.Appender.Grafana.Loki/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Log4Net.Appender.Grafana.Loki.svg)](https://www.nuget.org/packages/Log4Net.Appender.Grafana.Loki/)
[![License](https://img.shields.io/github/license/gabrielcerutti/log4net.Appender.Loki)](LICENSE)

A high-performance log4net appender that sends log messages directly to [Grafana Loki](https://grafana.com/oss/loki/), the horizontally-scalable, highly-available log aggregation system. This library provides seamless integration with Loki's HTTP API, allowing you to centralize your application logs with minimal configuration.

## 📋 Table of Contents

- [Features](#-features)
- [Compatibility](#-compatibility)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [Usage Examples](#-usage-examples)
- [Advanced Configuration](#-advanced-configuration)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

- **✅ Native Loki Integration** - Uses the latest Loki HTTP API [POST /loki/api/v1/push](https://grafana.com/docs/loki/latest/api/#push-log-entries-to-loki)
- **✅ JSON Formatting** - Structured logging with JSON message format
- **✅ High Performance** - Configurable buffering to minimize network overhead
- **✅ Secure Communication** - Built-in support for Basic Authentication
- **✅ GZip Compression** - Reduce bandwidth usage with automatic compression
- **✅ SSL/TLS Support** - Option to trust self-signed certificates for development
- **✅ Global Labels** - Add custom labels (environment, application name, etc.) to all log streams
- **✅ Dynamic Labels** - Declare any number of extra custom labels directly from your `log4net.config`
- **✅ Cross-Platform** - Supports .NET Framework 4.6.2+, .NET Core, and .NET 5+

## 🔧 Compatibility

| Platform | Version | Status |
|----------|---------|--------|
| .NET Framework | 4.6.2 - 4.8 | ✅ Supported |
| .NET Standard | 2.0+ | ✅ Supported |
| .NET Core | 2.0+ | ✅ Supported |
| .NET | 5.0+ | ✅ Supported |

**Dependencies:**
- log4net ≥ 3.3.0
- Newtonsoft.Json ≥ 13.0.3

## 📦 Installation

### NuGet Package Manager
```powershell
Install-Package Log4Net.Appender.Grafana.Loki
```

### .NET CLI
```bash
dotnet add package Log4Net.Appender.Grafana.Loki
```

### Package Reference
```xml
<PackageReference Include="Log4Net.Appender.Grafana.Loki" Version="1.0.0" />
```

**NuGet Package:** [Log4Net.Appender.Grafana.Loki](https://www.nuget.org/packages/Log4Net.Appender.Grafana.Loki/)

## 🚀 Quick Start

### 1. Configure log4net

Add the Loki appender to your `log4net.config` or `app.config`:

```xml
<log4net>
  <appender name="loki" type="Log4Net.Appender.Loki.LokiAppender, Log4Net.Appender.Grafana.Loki">
    <ServiceUrl value="http://localhost:3100" />
    <Application value="MyApp" />
    <Environment value="Production" />
  </appender>

  <root>
    <level value="INFO" />
    <appender-ref ref="loki" />
  </root>
</log4net>
```

### 2. Use log4net as usual

```csharp
using log4net;

public class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
        log4net.Config.XmlConfigurator.Configure();

        log.Info("Application started");
        log.Debug("Debug message");
        log.Warn("Warning message");
        log.Error("Error message");

        log.Info("Application finished");
    }
}
```

### 3. View logs in Grafana

Your logs will appear in Grafana Loki with labels:
- `application="MyApp"`
- `environment="Production"`
- `level="INFO"` (or DEBUG, WARN, ERROR, etc.)

## ⚙️ Configuration

### Basic Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ServiceUrl` | string | *Required* | Loki server URL (e.g., `http://localhost:3100`) |
| `Application` | string | `null` | Global label for application name |
| `Environment` | string | `null` | Global label for environment (Dev, Staging, Production) |
| `Label` | LokiLabel | `null` | Extra custom label added to all log streams (repeatable) |
| `BufferSize` | int | `512` | Number of log entries to buffer before sending |

### Advanced Configuration

```xml
<log4net>
  <appender name="loki" type="Log4Net.Appender.Loki.LokiAppender, Log4Net.Appender.Grafana.Loki">
    <!-- Required -->
    <ServiceUrl value="http://localhost:3100" />

    <!-- Global Labels -->
    <Application value="MyWebApp" />
    <Environment value="Production" />

    <!-- Extra Custom Labels (repeatable) -->
    <Label>
      <Key value="Team" />
      <Value value="Backend" />
    </Label>
    <Label>
      <Key value="Region" />
      <Value value="eu-west-1" />
    </Label>

    <!-- Performance -->
    <BufferSize value="100" />

    <!-- Authentication (if required) -->
    <BasicAuthUserName value="admin" />
    <BasicAuthPassword value="secret" />

    <!-- Compression -->
    <GZipCompression value="true" />

    <!-- SSL/TLS -->
    <TrustSelfSignedCerts value="false" />
  </appender>

  <root>
    <level value="ALL" />
    <appender-ref ref="loki" />
  </root>
</log4net>
```

### Configuration Properties Reference

#### `ServiceUrl` *(Required)*
The base URL of your Loki instance.
```xml
<ServiceUrl value="http://localhost:3100" />
<ServiceUrl value="https://loki.example.com" />
```

#### `Application`
Adds a custom `application` label to all log streams.
```xml
<Application value="MyWebService" />
```

#### `Environment`
Adds a custom `environment` label to all log streams.
```xml
<Environment value="Development" />
<Environment value="Staging" />
<Environment value="Production" />
```

#### `Label`
Adds an extra custom label to all log streams. Can be specified **multiple times** to add as many labels as needed. Each `<Label>` element requires a `Key` and a `Value` child element.
```xml
<Label>
  <Key value="Team" />
  <Value value="Backend" />
</Label>
<Label>
  <Key value="Region" />
  <Value value="eu-west-1" />
</Label>
```
All configured labels (including `Application`, `Environment`, and any `Label` entries) will appear on every log stream sent to Loki, making them available for filtering in Grafana.

#### `BufferSize`
Number of log entries to accumulate before sending to Loki. Higher values reduce network calls but increase memory usage.
```xml
<BufferSize value="512" /> <!-- Default -->
<BufferSize value="100" />  <!-- Lower latency -->
<BufferSize value="1000" /> <!-- Higher throughput -->
```

#### `BasicAuthUserName` & `BasicAuthPassword`
Credentials for Basic Authentication if your Loki instance requires it.
```xml
<BasicAuthUserName value="admin" />
<BasicAuthPassword value="mySecurePassword" />
```

#### `GZipCompression`
Enable GZip compression to reduce network bandwidth.
```xml
<GZipCompression value="true" />  <!-- Recommended for production -->
<GZipCompression value="false" /> <!-- Default -->
```

#### `TrustSelfSignedCerts`
Allow connections to Loki instances with self-signed SSL certificates. **Use only in development!**
```xml
<TrustSelfSignedCerts value="false" /> <!-- Default - Recommended -->
<TrustSelfSignedCerts value="true" />  <!-- Development only -->
```

## 💡 Usage Examples

### Example 1: .NET Framework Console Application

See the complete example in [`Example/`](Example/) folder.

```csharp
using System;
using log4net;

namespace ConsoleApp
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("Starting application");

            try
            {
                // Your application logic
                DoWork();
            }
            catch (Exception ex)
            {
                log.Error("Application error", ex);
            }

            log.Info("Application completed");
        }

        static void DoWork()
        {
            log.Debug("Doing work...");
            // Work logic here
        }
    }
}
```

### Example 2: .NET Core / .NET 6+ Application

See the complete example in [`Example.NetCore/`](Example.NetCore/) folder.

```csharp
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;

namespace NetCoreApp
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            log.Info("Application started");
            log.Warn("This is a warning");
            log.Error("This is an error");
            log.Info("Application finished");
        }
    }
}
```

### Example 3: ASP.NET Application

```xml
<!-- Web.config or app.config -->
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>

  <log4net>
    <appender name="loki" type="Log4Net.Appender.Loki.LokiAppender, Log4Net.Appender.Grafana.Loki">
      <ServiceUrl value="https://loki.mycompany.com" />
      <Application value="MyWebApp" />
      <Environment value="Production" />
      <BufferSize value="50" />
      <GZipCompression value="true" />
    </appender>

    <root>
      <level value="INFO" />
      <appender-ref ref="loki" />
    </root>
  </log4net>
</configuration>
```

```csharp
// Global.asax.cs
protected void Application_Start()
{
    log4net.Config.XmlConfigurator.Configure();
}
```

## 🔍 Advanced Configuration

### Multiple Appenders

You can combine the Loki appender with other appenders:

```xml
<log4net>
  <!-- Loki Appender -->
  <appender name="loki" type="Log4Net.Appender.Loki.LokiAppender, Log4Net.Appender.Grafana.Loki">
    <ServiceUrl value="http://localhost:3100" />
    <Application value="MyApp" />
  </appender>

  <!-- Console Appender -->
  <appender name="console" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>

  <!-- File Appender -->
  <appender name="file" type="log4net.Appender.RollingFileAppender">
    <file value="logs/app.log" />
    <appendToFile value="true" />
    <rollingStyle value="Size" />
    <maxSizeRollBackups value="5" />
    <maximumFileSize value="10MB" />
    <staticLogFileName value="true" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>

  <root>
    <level value="DEBUG" />
    <appender-ref ref="loki" />
    <appender-ref ref="console" />
    <appender-ref ref="file" />
  </root>
</log4net>
```

### Environment-Specific Configuration

```csharp
// Configure based on environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var configFile = new FileInfo($"log4net.{environment}.config");

if (!configFile.Exists)
{
    configFile = new FileInfo("log4net.config");
}

XmlConfigurator.Configure(logRepository, configFile);
```

## 🐛 Troubleshooting

### Logs not appearing in Loki

1. **Check Loki URL**: Ensure `ServiceUrl` is correct and Loki is running
   ```bash
   curl http://localhost:3100/ready
   ```

2. **Check network connectivity**: Verify firewall rules and network access

3. **Enable log4net internal debugging**:
   ```xml
   <configuration>
     <appSettings>
       <add key="log4net.Internal.Debug" value="true"/>
     </appSettings>
   </configuration>
   ```

4. **Check buffer size**: If `BufferSize` is large, logs may be delayed. Try a smaller value like `10` for testing.

### Authentication errors

If you see HTTP 401 errors:
- Verify `BasicAuthUserName` and `BasicAuthPassword` are correct
- Check if your Loki instance requires authentication

### SSL/TLS certificate errors

For self-signed certificates in **development only**:
```xml
<TrustSelfSignedCerts value="true" />
```

⚠️ **Never use `TrustSelfSignedCerts="true"` in production!**

### Performance issues

- **Increase `BufferSize`** to reduce network calls
- **Enable `GZipCompression`** to reduce bandwidth
- Consider using async appender wrapper if available

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/gabrielcerutti/log4net.Appender.Loki.git
   cd log4net.Appender.Loki
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run tests:
   ```bash
   dotnet test
   ```

### Guidelines

- Follow existing code style
- Add unit tests for new features
- Update documentation for any configuration changes
- Test against .NET Framework and .NET Core

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original implementation by [Anas El Hajjaji](https://github.com/anaselhajjaji)
- Maintained by [Gabriel Cerutti](https://github.com/gabrielcerutti)
- Built with [log4net](https://logging.apache.org/log4net/)
- Integrates with [Grafana Loki](https://grafana.com/oss/loki/)

## 🔗 Related Links

- [Grafana Loki Documentation](https://grafana.com/docs/loki/latest/)
- [log4net Documentation](https://logging.apache.org/log4net/release/manual/introduction.html)
- [NuGet Package](https://www.nuget.org/packages/Log4Net.Appender.Grafana.Loki/)
- [GitHub Repository](https://github.com/gabrielcerutti/log4net.Appender.Loki)
- [Report Issues](https://github.com/gabrielcerutti/log4net.Appender.Loki/issues)

---

**⭐ If you find this project useful, please consider giving it a star on GitHub! ⭐**
