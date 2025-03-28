# BlazorWire

BlazorWire is a .NET library that provides real-time communication capabilities for Blazor applications. It consists of two main components:

- `blazor-wire`: The client-side library for Blazor applications
- `blazor-wire-server`: The server-side component for handling real-time connections

## Features

- Real-time bidirectional communication
- Seamless integration with Blazor components
- Type-safe message passing
- Built on modern .NET technologies

## Installation

Add the NuGet package to your Blazor project:

```bash
dotnet add package BlazorWire
```

For the server component:

```bash
dotnet add package BlazorWire.Server
```

## Usage

Detailed usage instructions and examples coming soon.

## Building from Source

This project uses NX for build orchestration. To build:

1. Ensure you have the .NET SDK installed
2. Run the build command:

```bash
nx build blazor-wire
```

For the server component:

```bash
nx build blazor-wire-server
```

## Publishing

```shell
nx run-many --target=pack -p tag:blazor-wire -c production --extra-parameters="-p:Version=<version>"
```