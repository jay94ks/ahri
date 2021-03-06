## Ahri
_The Simplest Dependency Injection Framework_

_Use DI on Everywhere!_

Ahri is the alternative implementation that is compatible 
with `System.IServiceProvider` of the dotnet runtime and designed with "Simple is best" concept.
The users can make an application with minimal effort that is modular based on dependency injection.

1. Minimize dependencies.
2. Increase reusability.
3. Easier test driven development
4. High readability

### Installation
See https://www.nuget.org/packages/Ahri here.

### Extensions.

### Basic usage.
```
using System;
using Ahri
using Ahri.Core;

class Program {
    static void Main(string[] args) {
        var Registry = new ServiceCollection();
        
        // Register your service classes to registry like:
        Registry
            .AddSingleton<Application>() // --> Singleton.
            .AddScoped<Database>()       // --> Scoped.
            .AddTransient<DateTime>(_ => DateTime.Now); // --> Transient.
        
        // Then, Build the ServiceProvider instance like:
        var Services = Registry.BuildServiceProvider();
        
        // And enjoy with your service instances.
        Services.GetService<Application>().Run();
    }
}
```

### Scope branch.
```
IServiceProvider Services = .....;

var Factory = Services.GetRequiredService<IServiceScopeFactory>();
using (var Scope = Factory.CreateScope()) {
    var Service = Scope.ServiceProvider.GetService<MyScopedService>();
    
    // TODO: ...
}
```

### Scope branch with Scope-specific services.
```
IServiceProvider Services = .....;

var ScopeServices = new ServiceCollection();

// Register the Scope-specific services to ScopeServices.
ScopeServices.AddSingleton<MyScopeSpecific>();

var Factory = Services.GetRequiredService<IServiceScopeFactory>();
using (var Scope = Factory.CreateScope(ScopeServices)) { // <-- IMPORTANT.
    var Service = Scope.ServiceProvider.GetService<MyScopeSpecific>();

    // TODO: ...
}
```

### Plain instantiation with Dependency Injection.
```
IServiceProvider Service = .....;

var Injector = Services.GetRequiredService<IServiceInjector>();

// -- Creating an object.
var Object = Injector.Create(typeof(MyObject));

// -- Invoking an method.
var Method = typeof(MyObject).GetMethod("MyMethod");
var RetVal = Injector.Invoke(Method, Injector);
//     --> parameters will be resolved from the service provider.

```

### Hosting feature. (Ahri.Hosting package)
```
using System;
using System.Threading.Tasks;
using Ahri
using Ahri.Core;
using Ahri.Hosting;
using Ahri.Hosting.Builders;

class Program {
    static async Task Main(string[] args) {
        await new HostBuilder()
            .ConfigureServices(Registry => {
                // TODO: Register services here.
            })
            .Configure(Services => {
                // TODO: Configure service instances here.
            })
            .Build()
            .RunAsync();
    }
}
```

### Windows Service Hosting feature. (Ahri.Hosting.Windows package)
```
using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Hosting.Windows;

class Program {
    static async Task Main(string[] args) {
        await new HostBuilder()
            .EnableWindowsService()
            .ConfigureServices(Registry => {
                // TODO: Register services here.
            })
            .Configure(Services => {
                // TODO: Configure service instances here.
            })
            .Build()
            .RunAsync();
    }
}
```

### WinForm Hosting feature. (Ahri.Hosting.WinForm package)
```
using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Hosting.Winform;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program {
    static async Task Main(string[] args) {
        await new HostBuilder()
            .ConfigureServices(Registry => {
                // TODO: Register services here.
                Registry.AddSingletonForm<MyForm>();
            })
            .Configure(Services => {
                var Pump = Services.GetRequiredService<IWinFormMessagePump>();
                _ = Pump.InvokeAsync(() =>
                {
                    var Form = Services.GetRequiredService<MyForm>();

                    /* TODO: Write logic to mediate forms. */
                    Form.FormClosed += (_, _) =>
                    {
                        Services
                            .GetRequiredService<IHostLifetime>()
                            .Terminate();
                    };

                    Form.Show();
                });
            })
            .Build()
            .RunAsync();
    }

    class MyForm : Form {
        // ........................
    }
}

```

### Http.Hosting Feature.
```
using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Http.Hosting;
using Ahri.Http.Orb;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Networks.Http
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureHttpHost(Http =>
                {
                    Http.UseOrb() // --> Use `Orb` as Http Server.
                        .ConfigureHttpServer(Server =>
                        {
                            Server.Endpoint = new IPEndPoint(IPAddress.Any, 5000);
                        })

                        .ConfigureServices(Registry =>
                        {
                            // TODO: Register services here.
                        })

                        .Configure(App =>
                        {
                            // TODO: Adds Middlewares here.
                            App.Use((Context, Next) =>
                            {

                                return Next();
                            });

                            App.Configure(Services =>
                            {
                                // TODO: Do migration if needed. 
                            });
                        });

                })
                .Build()
                .RunAsync();
        }
    }
}
```

### Dockerization
Example Dockerfile:
```
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . /src
RUN ls -al /src
RUN dotnet restore "examples/Ahri.Examples.Dockerization/Ahri.Examples.Dockerization.csproj"
WORKDIR "/src/examples/Ahri.Examples.Dockerization"
RUN dotnet build "Ahri.Examples.Dockerization.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ahri.Examples.Dockerization.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Ahri.Examples.Dockerization.dll"]
```

Example docker-compose.yml:
```
version: '3.4'
services:
  ahri.examples.dockerization:
    image: ${DOCKER_REGISTRY-}ahriexamplesdockerization
    build:
      context: .
      dockerfile: examples/Ahri.Examples.Dockerization/Dockerfile
    ports:
      - "5000:5000"
```
