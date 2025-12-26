using Microsoft.AspNetCore.Http;

namespace HowlDev.Web.Helpers.WebSockets;

/// <summary>
/// The interface for the HowlDev.Web.Helpers.Websockets service.
/// </summary>
public interface IWebSocketService {
    Task RegisterSocket(HttpContext context, object key);

    Task SendSocketMessage(object key, string message);
}