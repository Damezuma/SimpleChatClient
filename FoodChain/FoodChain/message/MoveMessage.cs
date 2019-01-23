using System;
using System.Collections.Generic;
using System.Text;
using FoodChain;

namespace FoodChain.message
{
    class MoveMessage : message
    {
        private const byte MOVEFLAG = 0xa3;
        private App6.Opposite opposite;
        protected override byte Flag {
            get => MOVEFLAG;
           }
        public MoveMessage(App6.Opposite opposite)
        {
            
            this.opposite = opposite;
        }
        public override byte[] MessageBytes
        {
            get
            {
                byte[] toGo = new byte[2];
                toGo[0] = Flag;

                switch (this.opposite)
                {
                    case App6.Opposite.mountain:
                        toGo[1] = 0xb1;
                        break;
                    case App6.Opposite.river:
                        toGo[1] = 0xb2;
                        break;
                    case App6.Opposite.sky:
                        toGo[1] = 0xb3;
                        break;
                    case App6.Opposite.field:
                        toGo[1] = 0xb4;
                        break;
                }

                return toGo;
            }
            
        }
    }
}
