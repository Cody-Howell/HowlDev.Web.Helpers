using HowlDev.Web.Helpers.WebSockets;

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
