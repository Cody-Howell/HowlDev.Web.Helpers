using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace HowlDev.Web.Helpers.WebSockets;

/// <summary>
/// Register a socket by passing in an HttpContext value and the key to register with. Currently only 
/// supports sending messages.
/// </summary>
/// <typeparam name="T">Type for the keys in the inner ConcurrentDictionary</typeparam>
public class WebSocketService<T>(ILogger<WebSocketService<T>> logger) : IWebSocketService where T : notnull{
    private readonly ConcurrentDictionary<T, ConcurrentDictionary<string, WebSocket>> sockets = new();

    public async Task RegisterSocket(HttpContext context, T key) {
        var inner = sockets.GetOrAdd(key, _ => new ConcurrentDictionary<string, WebSocket>());
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await AddNewWebsocket(webSocket, inner);
    }

    public async Task RegisterSocket(HttpContext context, object key) {
        await RegisterSocket(context, (T)key);
    }

    public async Task SendSocketMessage(T key, string message) {
        if (!sockets.TryGetValue(key, out var inner)) {
            return;
        }
        await SendMessage(inner, message);
    }

    public async Task SendSocketMessage(object key, string message) {
        await SendSocketMessage((T)key, message);
    }

    private async Task AddNewWebsocket(WebSocket webSocket, ConcurrentDictionary<string, WebSocket> inner) {
        var connectionId = Guid.NewGuid().ToString();
        inner.TryAdd(connectionId, webSocket);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult? result = null;

        try {
            do {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
            } while (!(result?.CloseStatus.HasValue ?? false));
        } catch (Exception ex) {
            logger.LogError("WebSocket error: {message}", ex.Message);
        } finally {
            // Clean up this connection from the inner map
            inner.TryRemove(connectionId, out var _);

            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted) {
                try {
                    await webSocket.CloseAsync(result?.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result?.CloseStatusDescription, CancellationToken.None);
                } catch { }
            }
            try { webSocket.Dispose(); } catch { }
        }
    }

    private async Task SendMessage(ConcurrentDictionary<string, WebSocket> inner, string message) {
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        // Snapshot the connections to avoid enumeration issues
        var snapshot = inner.ToArray();

        foreach (var (id, socket) in snapshot) {
            if (socket == null) {
                inner.TryRemove(id, out var _);
                continue;
            }

            if (socket.State != WebSocketState.Open) {
                inner.TryRemove(id, out var _);
                try { socket.Dispose(); } catch { }
                continue;
            }

            try {
                await socket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
            } catch {
                inner.TryRemove(id, out var _);
                try { socket.Dispose(); } catch { }
            }
        }
    }
}