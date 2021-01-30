using System;
using System.Text;
using UnityEngine;
using System.Net;
using System.IO;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net;
using System.Net.Sockets;
#endif

namespace HoloLab.LogBroadcaster
{
    /// <summary>
    /// ログをUDP送信する
    /// </summary>
    public class LogBroadcaster : MonoBehaviour
    {
        /// <summary>
        /// スタックトレースを送るかどうか
        /// </summary>
        [SerializeField]
        bool isSendStackTrace = false;

        /// <summary>
        /// ブロードキャストするかどうか
        /// </summary>
        [SerializeField]
        bool isBroadcast = true;

        /// <summary>
        /// 送信先IPアドレス(ブロードキャストではない場合)
        /// </summary>
        [SerializeField]
        string targetIpAddress = "192.168.11.100";

        /// <summary>
        /// 送信ポート番号
        /// </summary>
        [SerializeField]
        int port = 20080;

#if WINDOWS_UWP        /// <summary>
        /// ソケット
        /// </summary>
        DatagramSocket socket;

        /// <summary>
        /// ストリーム
        /// </summary>
        IOutputStream stream;

        /// <summary>
        /// 書き込み
        /// </summary>
        StreamWriter writer;

        /// <summary>
        /// ブロードキャストアドレス
        /// </summary>
        string broadcastAddress;
#else
        /// <summary>
        /// UDPクライアント
        /// </summary>
        private UdpClient udpclient;
#endif

        /// <summary>
        /// 送信ログフォーマット(JSONにする)
        /// </summary>
        public class LogMessage
        {
            public string condition;
            public string stackTrace;
            public string type;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Awake()
        {
#if WINDOWS_UWP
            Task.Run(async () =>
            {
                socket = new DatagramSocket();
                broadcastAddress = IPAddress.Broadcast.ToString();

                stream = await socket.GetOutputStreamAsync(new HostName(isBroadcast ? broadcastAddress : targetIpAddress), port.ToString());
                writer = new StreamWriter(stream.AsStreamForWrite());
            }).Wait();
#else
            udpclient = new UdpClient();
            udpclient.EnableBroadcast = isBroadcast;
            udpclient.Connect(new IPEndPoint((isBroadcast ? IPAddress.Broadcast : IPAddress.Parse(targetIpAddress)), port));
#endif

            Application.logMessageReceived += Application_logMessageReceived;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        private void OnDestroy()
        {
            Application.logMessageReceived -= Application_logMessageReceived;

#if WINDOWS_UWP
            writer?.Dispose();
            stream?.Dispose();
            socket?.Dispose();
#else
            udpclient?.Dispose();
#endif
        }

        /// <summary>
        /// ログをフックする
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            var message = new LogMessage() {
                condition = condition,
                type = type.ToString()
            };

            if (isSendStackTrace)
            {
                message.stackTrace = stackTrace;
            }

            var json = JsonUtility.ToJson(message);

#if WINDOWS_UWP
            Task.Run(async () => {
                await writer?.WriteAsync(json);
                await writer?.FlushAsync();
            }).Wait();
#else
            var buffer = Encoding.UTF8.GetBytes(json);
            udpclient.Send(buffer, buffer.Length);
#endif
        }
    }
}
