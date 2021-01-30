using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

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

#if UNITY_UWP
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

#if UNITY_UWP
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

#if UNITY_UWP
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

#if UNITY_UWP
#else
            var buffer = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));
            udpclient.Send(buffer, buffer.Length);
#endif
        }

#if UNITY_UWP
        Task.Run(async () =>
        {
            DatagramSocket socket = new DatagramSocket();
            string Address = IPAddress.Broadcast.ToString();
            var datagram = await socket.GetOutputStreamAsync(new HostName(Address), port.ToString());
            writer = new StreamWriter(datagram.AsStreamForWrite());
        });
#else
#endif
    }
}
