﻿using System.Net.Sockets;

namespace QuickChatter.Models
{
    public class ConnectedClient
    {
        public Guid Id { get; set; }
        public TcpClient Client { get; set; }
        public string Username { get; set; }
        public string Ip { get; set; }
        public bool IsAvailable { get; set; }
    }
}
