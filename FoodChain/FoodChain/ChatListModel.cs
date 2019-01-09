using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App6
{
    public class ChatListModels : INotifyPropertyChanged
    {
        private const string HOST = "192.168.0.4";
        private const int PORT = 8080;

        private ObservableCollection<string> chatList;
        private string message;
        private Socket senderSocket;
        private Socket receiveSocket;
        private int uid;
        public event PropertyChangedEventHandler PropertyChanged;
        private Task sendTask = null;
        private Task receiverTask = null;
        public ChatListModels()
        {
            chatList = new ObservableCollection<string>();
            ConnectToServer();
        }
        private async void ConnectToServer()
        {
            receiveSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await receiveSocket.ConnectAsync(HOST, PORT);
            var uidBytes = BitConverter.GetBytes(uid);
            int s = 0;
            do
            {
                s += receiveSocket.Send(uidBytes, s, uidBytes.Length - s, SocketFlags.None);
            }
            while (s != uidBytes.Length);
            s = 0;
            do
            {
                s += receiveSocket.Receive(uidBytes, s, uidBytes.Length - s, SocketFlags.None);
            }
            while (s != uidBytes.Length);
            uid = BitConverter.ToInt32(uidBytes, 0);

            senderSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await senderSocket.ConnectAsync(HOST, PORT);
            s = 0;
            do
            {
                s += senderSocket.Send(uidBytes, s, uidBytes.Length - s, SocketFlags.None);
            }
            while (s != uidBytes.Length);
            s = 0;
            do
            {
                s += senderSocket.Receive(uidBytes, s, uidBytes.Length - s, SocketFlags.None);
            }
            while (s != uidBytes.Length);
            if (uid != BitConverter.ToInt32(uidBytes, 0))
            {
                throw new Exception("Could not connect to server!");
            }
            else
            {

            }
            //
            receiverTask = Task.Run(() =>
            {
                var stream = new NetworkStream(receiveSocket);
                var chunk = new byte[4096];
                var buffer = new List<byte>();
                int ETB = 0;
                while (true)
                {
                    int readByte = stream.Read(chunk, 0, 4096);
                    if (readByte == 0) continue;
                    buffer.AddRange(chunk.Take(readByte));
                    while (true)
                    {
                        ETB = buffer.FindIndex((byte it) => it == 0x17);
                        if (ETB == -1)
                        {
                            break;
                        }
                        if (ETB != 0)
                        {
                            string text = Encoding.UTF8.GetString(buffer.ToArray(), 0, ETB);
                            chatList.Add(text);
                        }
                        buffer.RemoveRange(0, ETB + 1);

                        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChatList"));
                    }
                }

                //receiveSocket.Receive()
            });
        }
        public async void SendMessageToServer()
        {
            if (message.Trim().Length == 0) return;
            message = message.Trim();
            if (sendTask != null)
            {
                await sendTask;
            }
            sendTask = Task.Run(() =>
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                Message = "";

                int s = 0;
                do
                {
                    s += senderSocket.Send(bytes, s, bytes.Length - s, SocketFlags.None);
                }
                while (s != bytes.Length);
                bytes[0] = 0x17;
                while (senderSocket.Send(bytes, 0, 1, SocketFlags.None) != 1) ;
                sendTask = null;
            });
            //sendTask.Start();
        }
        public Command SendCommand
        {
            get => new Command(() => SendMessageToServer());
        }
        public string Message
        {
            get => message;
            set
            {

                message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            }

        }
        public ObservableCollection<string> ChatList { get => chatList; }

    }
}
