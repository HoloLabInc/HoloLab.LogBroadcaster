using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LogBroadcasterReciever
{
    /// <summary>
    /// ログの受信
    /// </summary>
    public class LogBroadcasterReciever
    {
        /// <summary>
        /// ログ受信イベント
        /// </summary>
        public event EventHandler<LogMessage> LogReceived;

        /// <summary>
        /// ログ受信処理完了イベント
        /// </summary>
        public event EventHandler LogReceivedComplete;

        /// <summary>
        /// UDPクライアント
        /// </summary>
        UdpClient udpclient;

        /// <summary>
        /// キャンセル用トークン
        /// </summary>
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// ログ受信処理の開始
        /// </summary>
        /// <param name="port">UDP受信ポート番号</param>
        public void Start(int port)
        {
            Task.Run(() =>
            {
                var remote = new IPEndPoint(IPAddress.Any, port);

                using (udpclient = new UdpClient(port))
                {
                    try
                    {
                        do
                        {
                            byte[] bytes = udpclient.Receive(ref remote);
                            string json = Encoding.UTF8.GetString(bytes);
                            var message = JsonSerializer.Deserialize<LogMessage>(json);

                            LogReceived?.Invoke(this, message);
                        } while (!tokenSource.Token.IsCancellationRequested);
                    }
                    catch (Exception)
                    {
                    }

                    LogReceivedComplete?.Invoke(this, null);
                }
            });
        }

        /// <summary>
        /// ログ受信のキャンセル
        /// </summary>
        public void Cancel()
        {
            udpclient.Dispose();
            tokenSource.Cancel();
        }
    }
}
