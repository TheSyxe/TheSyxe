using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syxebot.Data
{
    class ChatMessage
    {
        private ChatSharp.IrcChannel _channel;
        private string _message;
        private string _nick;

        public int Identifiers
        {
            get { return MessageParams.Count(p => p != null); }
        }

        public string[] MessageParams
        {
            get { return _message.Split(' ').ToArray(); }
        }


        public ChatSharp.IrcChannel Channel
        {
            get { return _channel; }
            private set { _channel = value; }
        }
        public string Message
        {
            get { return _message; }
            private set { _message = value; }
        }
        public string Nick
        {
            get { return _nick; }
            set { _nick = value; }
        }



        public ChatMessage(ChatSharp.IrcChannel channel, ChatSharp.Events.PrivateMessageEventArgs e)
        {
            this.Channel = channel;
            this.Message = e.PrivateMessage.Message;
            this.Nick = e.PrivateMessage.User.Nick;
        }

    }
}
