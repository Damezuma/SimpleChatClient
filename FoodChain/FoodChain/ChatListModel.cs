using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using FoodChain.message;


namespace App6
{
    public enum Room { Field, Mountain, Sky, River}
    public enum FLAG { chat, attack, move, camouflage, spy, predict}
    public enum Opposite {mountain, sky, river, field, lion, alligator,
        eagle,
        hyena,
        snake,
        chameleon,
        deer,
        otter,
        rabbit,
        bronzeDuck,
        crow,
        crocodileBird,
        rat,
        no
    }
    public class ChatListModel : INotifyPropertyChanged
    {
        private const string HOST = "192.168.0.4";
        private const int PORT = 8080;
        private const byte NOLOG = 0xa0;
        private const byte CHATFLAG = 0xa1;
        private const byte LOBBYCHAT = 0xa1;
        private const byte MTCHAT = 0xa2;
        private const byte FDCHAT = 0xa3;
        private const byte SKYCHAT = 0xa4;
        private const byte RVCHAT = 0xa5;
        private const byte GHOST = 0xa6;


        private ObservableCollection<string> chatLists;
        private string message;
        private Socket senderSocket;
        private Socket receiveSocket;
        private int uid;
        public event PropertyChangedEventHandler PropertyChanged;
        private Task sendTask = null;
        private Task receiverTask = null;
        public ChatListModel()
        {

            chatLists = new ObservableCollection<string>();
            
            
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
                            var test = buffer.GetRange(0, 1).ToArray();
                            
                            if(test[0] == CHATFLAG)
                            {
                                var messageBuffer = buffer.GetRange(1, buffer.Count - 1);
                                string text = Encoding.UTF8.GetString(messageBuffer.ToArray(), 0, ETB - 1);
                                chatLists.Add(text);

                            }
 
                        }
                        buffer.RemoveRange(0, ETB + 1);

                        //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChatList"));
                    }
                }

                //receiveSocket.Receive()
            });
        }
        public async void SendMessageToServer(FLAG flag, Opposite opposite)
        {
            
            if (sendTask != null)
            {
                await sendTask;
            }
            sendTask = Task.Run(() =>
            {
                byte[] sendToBytes = new byte[1];

                message msg = null;
                switch (flag)
                {
                    case FLAG.chat:
                        {
                            if (this.message.Trim().Length == 0) return;
                            string message = this.message.Trim();
                            this.Message = "";
                            msg = new ChatMessage(message);                      
                            break;
                        }
                    case FLAG.move:
                             msg = new MoveMessage(opposite);
                            break;
                }
                if(msg == null)
                {
                    sendToBytes[0] = NOLOG;
                }
                else
                {
                    sendToBytes = msg.MessageBytes;
                }

                int s = 0;
                do
                {
                    s += senderSocket.Send(sendToBytes, s, sendToBytes.Length - s, SocketFlags.None);
                }
                while (s != sendToBytes.Length);

                byte[] bytes = new byte[1];
                bytes[0] = 0x17;

                while (senderSocket.Send(bytes, 0, 1, SocketFlags.None) != 1) ;
                sendTask = null;

            });
            //sendTask.Start();
        }
        public Command Send
        {
            get => new Command(() => SendMessageToServer(FLAG.chat,Opposite.no));
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
        public Room Room
        {
            set
            {
                switch(value)
                {
                    case Room.Field:
                        SendMessageToServer(FLAG.move, Opposite.field);
                        break;
                    case Room.Mountain:
                        SendMessageToServer(FLAG.move, Opposite.mountain);
                        break;
                    case Room.Sky:
                        SendMessageToServer(FLAG.move, Opposite.sky);
                        break;
                    case Room.River:
                        SendMessageToServer(FLAG.move, Opposite.river);
                        break;
                }
            }
        }

        public ObservableCollection<string> ChatLists { get => chatLists; }

    }
}
