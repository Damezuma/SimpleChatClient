using System;
using System.Collections.Generic;
using System.Text;

namespace FoodChain.message
{
    public abstract class message
    {
        protected abstract byte Flag { get; }
        public abstract byte[] MessageBytes { get; }

    }
}
