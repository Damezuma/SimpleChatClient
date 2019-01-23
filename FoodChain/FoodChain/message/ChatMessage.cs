using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodChain.message
{
    class ChatMessage : message
    {
        private const byte CHATFLAG = 0xa1;
        private string message;

        protected override byte Flag
        {
            get => CHATFLAG;
        }

        public ChatMessage(string message)
        {
            this.message = message;
        }
        public override byte[] MessageBytes
        {
            get
            {
                byte[] roomState = new byte[1];
                roomState[0] = Flag;

                var bytes = Encoding.UTF8.GetBytes(this.message);


                var sendToBytes = roomState.Concat(bytes).ToArray();
                return sendToBytes;
            }

        }
    }
}
