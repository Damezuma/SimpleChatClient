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
        private const byte LOCALFLAG = 0xed;
        private const byte LOBBYCHAT = 0xa1;
        private const byte MTCHAT = 0xa2;
        private const byte FDCHAT = 0xa3;
        private const byte SKYCHAT = 0xa4;
        private const byte RVCHAT = 0xa5;
        private const byte GHOST = 0xa6;
    

        private ObservableCollection<string>[] chatLists = new ObservableCollection<string>[6];
        private string[] messages = new string[6];
        private Socket senderSocket;
        private Socket receiveSocket;
        private int uid;
        public event PropertyChangedEventHandler PropertyChanged;
        private Task sendTask = null;
        private Task receiverTask = null;
        public ChatListModels()
        {
            for(int i = 0; i < 6; i++)
            {
                chatLists[i] = new ObservableCollection<string>();
            }
            
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
                            var test = buffer.GetRange(0, 2).ToArray();
                            if(test[0] == LOCALFLAG)
                            {
                                var messageBuffer = buffer.GetRange(4, buffer.Count - 4);
                                string text = Encoding.UTF8.GetString(messageBuffer.ToArray(), 0, ETB - 4);

                                switch (test[1])
                                {
                                    case LOBBYCHAT:
                                        chatLists[0].Add(text);
                                        break;
                                    case MTCHAT:
                                        chatLists[1].Add(text);
                                        break;
                                    case FDCHAT:
                                    case SKYCHAT:
                                    case RVCHAT:
                                        break;
                                }
                            }
                            

                            
                                                      
                        }
                        buffer.RemoveRange(0, ETB + 1);

                        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChatList"));
                    }
                }

                //receiveSocket.Receive()
            });
        }
        public async void SendMessageToServer(int flag)
        {
            if (messages[flag].Trim().Length == 0) return;
            string message = messages[flag].Trim();
            if (sendTask != null)
            {
                await sendTask;
            }
            sendTask = Task.Run(() =>
            {
                byte[] roomState = new byte[2];
                roomState[0] = LOCALFLAG;
        
                var bytes = Encoding.UTF8.GetBytes(message);
                


                switch (flag)
                {
                    case 0:
                        roomState[1] =LOBBYCHAT;
                        LobbyMessage = "";
                        break;
                    case 1:
                        roomState[1] = MTCHAT;
                        MWTNMessage = "";
                        break;
                }

                var sendToBytes = roomState.Concat(bytes).ToArray();

                int s = 0;
                do
                {
                    s += senderSocket.Send(sendToBytes, s, sendToBytes.Length - s, SocketFlags.None);
                }
                while (s != sendToBytes.Length);
                bytes[0] = 0x17;
                while (senderSocket.Send(bytes, 0, 1, SocketFlags.None) != 1) ;
                sendTask = null;
            });
            //sendTask.Start();
        }
        public Command SendInLobby
        {
            get => new Command(() => SendMessageToServer(0));
        }

        public Command SendInMWTN
        {
            get => new Command(() => SendMessageToServer(1));
        }

        public string LobbyMessage
        {
            get => messages[0];
            set
            {

                messages[0] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LobbyMessage"));
            }

        }

        public string MWTNMessage
        {
            get => messages[1];
            set
            {

                messages[1] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MWTNMessage"));
            }

        }

        public ObservableCollection<string>[] ChatLists { get => chatLists; }

    }
}
