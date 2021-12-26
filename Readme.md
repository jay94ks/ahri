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

### Developing features:
1. Dockerization.
2. Etc...........