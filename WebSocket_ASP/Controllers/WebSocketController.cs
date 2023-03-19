using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebSocketController : Controller
{
    public static List<System.Net.WebSockets.WebSocket> connections = new List<System.Net.WebSockets.WebSocket>();
    WebSocketServer wssv = new WebSocketServer("ws://127.0.0.1:7890");
    [HttpGet("/ws")]
    public async Task Get()
    {
        //是否為webSocket請求 如果是則加入等待
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            PlayerRef.id = generateID();
            connections.Add(webSocket);
            Console.WriteLine("New Client Connected");
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(System.Net.WebSockets.WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        //等待接收訊息
        var receiveResult = await webSocket.ReceiveAsync(
             new ArraySegment<byte>(buffer), CancellationToken.None);
        //檢查是否為連線狀態


        while (!receiveResult.CloseStatus.HasValue)
        {
            //訊息發到前端

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].State != System.Net.WebSockets.WebSocketState.Open)
                {
                    continue;
                }
                await connections[i].SendAsync(
               new ArraySegment<byte>(buffer, 0, receiveResult.Count),
               receiveResult.MessageType,
               receiveResult.EndOfMessage,
               CancellationToken.None);
            }

            //繼續等待接收訊息
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

        }
        //關閉連線

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription, CancellationToken.None);


    }
    public string generateID()
    {
        return Guid.NewGuid().ToString("N");
    }
}

