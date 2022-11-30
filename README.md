# log4net Loki Appender

This appender will allow log4net to be configured to send log messages to Loki directly.

[![Dot Net Framework 4.6.2 (Build)](https://github.com/gabrielcerutti/log4net.Appender.Loki/actions/workflows/netframework.build.yml/badge.svg)](https://github.com/gabrielcerutti/log4net.Appender.Loki/actions/workflows/netframework.build.yml)

## Installation

The log4net.Appender.LokiAppender NuGet [package can be found here](https://www.nuget.org/packages/log4net.Appender.Loki/). Alternatively you can install it via one of the following commands below:

NuGet command:
```bash
Install-Package Log4Net.Appender.Loki
```

## Log4net configuration

Sample Log4net config:

```xml
<log4net>
  <appender name="loki" type="log4net.Appender.LokiAppender, log4net.Appender.Loki">
    <Environment value="Development" /> <!-- Global label to be added to the log stream -->
    <Application value="WebApp" /> <!-- Global label to be added to the log stream -->
    <BufferSize  value="3" /> <!-- To configure the buffer size, default: 512 -->
    <ServiceUrl value="http://localhost:3100" /> <!-- Loki URL -->
    <BasicAuthUserName value="username" /> <!-- To be added if basic authent enabled  -->
    <BasicAuthPassword value="password" /> <!-- To be added if basic authent enabled  -->
    <GZipCompression value="true" /> <!-- To compress the post request using GZip compression -->
    <TrustSelfSignedCerts value="false" /> <!-- To trust self signed certificates. Default: false -->
  </appender>
</log4net>
```
