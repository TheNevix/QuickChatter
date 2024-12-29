using Newtonsoft.Json;
using QuickChatter.Models;
using QuickChatter.Models.Settings;
using QuickChatter.Server.Settings;
using System.Text;

namespace QuickChatter.Server
{
    public static class Broadcaster
    {
        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendListOfConnectedUsers(List<ConnectedClient> connectedClients, ConnectedClient requestingClient)
        {
            var mappedClients = new List<User>();

            foreach (var connectedClient in connectedClients)
            {
                mappedClients.Add(new User { Ip = connectedClient.Ip, Username = connectedClient.Username, IsAvailable = connectedClient.IsAvailable });
            }

            string jsonClients = JsonConvert.SerializeObject(mappedClients);

            //Serialze the clients list to a json string
            string message = $"{ResponseCode.Connected}|{jsonClients}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            requestingClient.Client.GetStream().WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendConversationInvite(ConnectedClient accepter, ConnectedClient inviter)
        {
            //Send a message to the accepting client
            string message = $"{ResponseCode.InviteReceived}|{ResponseMessage.AcceptInviteMessage.Replace("USERNAME", inviter.Username)}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            //Send
            accepter.Client.GetStream().WriteAsync(data, 0, data.Length);
        }
    }
}
