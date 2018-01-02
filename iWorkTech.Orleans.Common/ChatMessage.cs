using System;
using System.Collections.Generic;
using System.Text;

namespace iWorkTech.Orleans.Common
{
    public class ChatMessage
    {
        public ChatMessage()
        {
        }

        public ChatMessage(int chatId, string name, string message)
        {
            ChatId = chatId;
            Name = name;
            Message = message;
        }

        public int ChatId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
