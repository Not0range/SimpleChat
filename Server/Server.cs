using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        TcpListener server;
        public Status status;
        Action<string> logger;
        Dictionary<User, Socket> users;

        public Server(Action<string> logger)
        {
            status = Status.Waiting;
            server = new TcpListener(IPAddress.Any, Program.port);
            this.logger = logger;
            users = new Dictionary<User, Socket>();
        }

        public async Task Start()
        {
            status = Status.Working;
            logger("Запуск сервера");
            server.Start(15);
            while (true)
            {
                if (status == Status.Working)
                    await server.AcceptSocketAsync().ContinueWith(Handler);
                else
                    break;
            }
        }

        public async Task Stop()
        {
            status = Status.Stoping;
            while (!server.Pending()) ;
            server.Stop();
            logger("Остановка сервера");
        }

        private async Task Handler(Task<Socket> task)
        {
            var message = GetMessage(task.Result);
            if (message == null)
                return;
            switch(message.messageType)
            {
                case Message.MessageType.Heartbeat:
                    break;
                case Message.MessageType.Disconect:
                    break;
                case Message.MessageType.Request:
                    break;
                case Message.MessageType.Send:
                    break;
            }
            return;
        }

        private Message GetMessage(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int count;
            if((count = socket.Receive(buffer)) > 0)
            {
                switch(buffer[0])
                {
                    case 0:
                        return new Message(Message.MessageType.Heartbeat, Encoding.UTF8.GetString(buffer, count - 1, 1));
                    case 1:
                        return new Message(Message.MessageType.Disconect, Encoding.UTF8.GetString(buffer, count - 1, 1));
                    case 2:
                        return new Message(Message.MessageType.Request, "");
                    case 3:
                        return new Message(Message.MessageType.Send, Encoding.UTF8.GetString(buffer, count - 1, 1));
                }
            }
            return null;
        }

        private void HandleHeartbeat(Socket socket, string content)
        {
            User current = null;
            foreach (var u in users.Keys)
            {
                if (u.username == content)
                {
                    current = u;
                    break;
                }
            }

            if (current == null)
            {
                users.Add(new User(content, DateTime.Now), socket);
                socket.Send(new byte[1] { 0 });
            }
            else if (users[current] == socket)
                current.lastTime = DateTime.Now;
            else
            {
                socket.Send(new byte[1] { 1 });
            }
        }

        private void HandleDisconect(Socket socket, string content)
        {
            User current = null;
            foreach (var u in users.Keys)
            {
                if (u.username == content)
                {
                    current = u;
                    break;
                }
            }
            if (current != null && socket == users[current])
            {
                users.Remove(current);
                socket.Send(new byte[1] { 0 });
                socket.Disconnect(true);
            }
            else
            {
                socket.Send(new byte[1] { 1 });
            }
        }

        private void HandleRequest(Socket socket)
        {
            StringBuilder list = new StringBuilder();
            foreach (var key in users.Keys)
                list.AppendLine(key.username);
            socket.Send(Encoding.UTF8.GetBytes(list.ToString()));
        }

        private void HandleSend(Socket socket, string content)
        {

        }

        private class Message
        {
            public MessageType messageType;

            public string content;

            public Message(MessageType messageType, string content)
            {
                this.messageType = messageType;
                this.content = content;
            }

            public enum MessageType
            {
                Heartbeat,
                Disconect,
                Request,
                Send,
            }
        }

        private class User
        {
            public string username;

            public DateTime lastTime;

            public User(string username, DateTime lastTime)
            {
                this.username = username;
                this.lastTime = lastTime;
            }
        }
    }

    enum Status
    {
        Waiting,
        Working,
        Stoping
    }
}
