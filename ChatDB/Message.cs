using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDB
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual UserData Sender { get; set; }
        public virtual UserData Receiver { get; set; }
    }
}
