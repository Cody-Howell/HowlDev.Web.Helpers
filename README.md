[DbConnector](https://www.nuget.org/packages/HowlDev.Web.Helpers.DbConnector): ![NuGet Version](https://img.shields.io/nuget/v/HowlDev.Web.Helpers.DbConnector)
[WebSockets](https://www.nuget.org/packages/HowlDev.Web.Helpers.WebSockets): ![NuGet Version](https://img.shields.io/nuget/v/HowlDev.Web.Helpers.WebSockets)


# HowlDev.Web.Helpers
Contains a few helpers used often in web projects. 

[Find a link to the wiki here](https://wiki.codyhowell.dev/web.helpers). 

## HowlDev.Web.Helpers.DbConnecter

This is a single class that has 4 methods that helps deal with thread pooling from Dapper. It is supposed to be used like the following: 

```csharp
public Task<IEnumerable<Account>> GetAllUsersAsync() =>
    conn.WithConnectionAsync(async conn => {
        var GetUsers = "select p.id, p.accountName, p.role from \"HowlDev.User\" p order by 1";
        try {
            return await conn.QueryAsync<Account>(GetUsers);
        } catch {
            return [];
        }
    }
);
```

This is a method that returns like a get call, with the important `conn.WithConnectionAsync(async conn => {...})` call. In my experience, you run into thread pool locking pretty quick, so it's best just to start by using this. 

AI happens to be pretty good at converting old calls into this format if you give it to them. 

## Changelog

1.0.1 (12/15/25)

- Added workflow file
- Targeted Net8.0 instead

1.0 (12/13/25)

- Created


## HowlDev.Web.Helpers.WebSockets

This provides a WebSocketService class for use in APIs. This uses the simple WebSocket system provided by C# with no extra dependencies. It is designed as a single-service socket registration (`app.Map`) and sending messages to only those "subscribed" (which are the ones assigned to the service key). The entire system can be used by the sample code below. (Included in the TestingAPI in the WebSocket directory is a sample html page that displays the minimum requirement to use the socket).

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddWebSocketService<int>();

var app = builder.Build();
app.UseWebSockets();

app.Map("/ws/{id}", async (IWebSocketService service, HttpContext context, int id) => {
    await service.RegisterSocket(context, id);
});

app.MapGet("/post/{id}", async (IWebSocketService service, int id) => {
    await service.SendSocketMessage(id, $"This is the message: coming from id {id} at time {DateTime.Now}");
});

app.Run();
```

Add the WebSocketService with the key type in the Builder section, the _include the UseWebSockets middleware_, then just inject the service. I provided the interface that is generic (so you won't get type hints), but you can also request specific versions of the service by using types. So: 

```csharp
app.MapGet("/post/{id}", async (WebSocketService<int> service, int id) => {
    await service.SendSocketMessage(id, $"This is the message: coming from id {id} at time {DateTime.Now}");
});
```

I'm going to be looking into adding additional parts for the registration method to hopefully configure multiple services if needed, possibly as a group, but you should know that the only restriction for the type is that it is `notnull` (the only requirement for the dictionary keys). So you could theoretically use more complex objects to register sockets, but I would recommend the primitives `int` and `string`. 


## Changelog

1.0 (12/15/25)

- Created
