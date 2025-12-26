using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HowlDev.Web.Helpers.WebSockets;

public static class WebSocketBuilderExtensions {
    /// <summary>
    /// Adds a WebSocketService to the DI container. The type argument determines what the key type is for the 
    /// ConcurrentDictionary inside. <br/>
    /// When calling in your endpoint, call the IWebSocketService to perform your calls; otherwise you would need to 
    /// specify your types in every endpoint you called. This is designed only to register one service. <br/>
    /// ENSURE YOU CALL the <code>app.UseWebSockets();</code> middleware after 
    /// </summary>
    public static WebApplicationBuilder AddWebSocketService<T>(this WebApplicationBuilder builder) where T : notnull {
        builder.Services.AddSingleton<WebSocketService<T>>();
        builder.Services.AddSingleton<IWebSocketService>(sp => sp.GetRequiredService<WebSocketService<T>>());

        return builder;
    }
}
