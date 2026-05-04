using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Policy;
using CefSharp.DevTools.Security;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Chump_kuka
{
    internal class TcpListenerManager
    {
        private TaskCompletionSource<bool> startCompletionSource = new TaskCompletionSource<bool>(); // 用來通知外部程式伺服器啟動完成
        private NetworkStream stream;
        private int listen_port;

        public bool IsRunning { get; private set; } = false;

        public event EventHandler<TcpMessageEventArgs> MessageReceived;     // 接收客戶端訊息時觸發的事件
        public event EventHandler<TcpConnectionEventArgs> ClientConnected;      // 新的客戶端連線時觸發的事件

        public TcpListenerManager(int port)
        {
            listen_port = port;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, listen_port);
            listener.Start();

            // 當伺服器啟動並開始監聽時，設定 TaskCompletionSource 為成功
            startCompletionSource.SetResult(true);
            Console.WriteLine("C# TCP Server started...");
            IsRunning = listener.Server.IsBound;

            Task.Run(async () =>
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    // 獲取客戶端的 IP 地址
                    IPEndPoint remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    ClientConnected?.Invoke(this, new TcpConnectionEventArgs(client, remoteEndPoint));

                    // 開新 Task 處理客戶端
                    await HandleClientAsync(client);
                }
            });
        }

        /// <summary>
        /// 這個方法用來等待伺服器啟動並開始監聽
        /// </summary>
        /// <returns></returns>
        public Task WaitForServerToStartAsync()
        {
            return startCompletionSource.Task; // 回傳 Task，讓外部程式可以 await 這個 Task，直到伺服器啟動完成
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine("Client connected.");
            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true) // 允許持續接收多筆資料
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // 連線已關閉

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received from Node.js: " + message);

                        MessageReceived?.Invoke(this, new TcpMessageEventArgs(stream, message));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Received Error: " + ex.ToString());
                        stream = client.GetStream();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                IsRunning = false;
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        // 發送訊息的輔助方法
        public async Task SendMessageAsync(string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            if (stream != null)
            {
                await stream.WriteAsync(response, 0, response.Length);
                Console.WriteLine("Sent to Node.js: " + message);
            }
        }
    }
}
