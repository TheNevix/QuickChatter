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
                mappedClients.Add(new User { Id = connectedClient.Id, Ip = connectedClient.Ip, Username = connectedClient.Username, IsAvailable = connectedClient.IsAvailable });
            }

            string jsonClients = JsonConvert.SerializeObject(mappedClients);

            //Serialze the clients list to a json string
            string message = $"{ResponseCode.Connected}|{jsonClients}|{requestingClient.Id}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            requestingClient.Client.GetStream().WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendNewOnlineUser(List<ConnectedClient> connectedClients, ConnectedClient newClient)
        {
            var mappedClient = new User
            {
                Id = newClient.Id,
                Ip = newClient.Ip,
                IsAvailable = newClient.IsAvailable,
                Username = newClient.Username,
            };

            string jsonClient = JsonConvert.SerializeObject(mappedClient);

            //Send a message to the accepting client
            string message = $"{ResponseCode.ConnectedUser}|{jsonClient}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            //Send
            foreach (var client in connectedClients)
            {
                if (client.Username == newClient.Username || client.Client.Connected == false)
                {
                    continue;
                }

                client.Client.GetStream().WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendUpdatedClients(List<ConnectedClient> connectedClients, List<ConnectedClient> updatedClients)
        {
            var mappedClients = new List<User>();

            foreach (var updatedClient in updatedClients)
            {
                mappedClients.Add(new User { Id = updatedClient.Id, Ip = updatedClient.Ip, Username = updatedClient.Username, IsAvailable = updatedClient.IsAvailable });
            }

            string jsonUpdatedClients = JsonConvert.SerializeObject(mappedClients);

            //Create the message
            string message = $"{ResponseCode.UpdatedUsers}|{jsonUpdatedClients}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            ////Send
            foreach (var client in connectedClients)
            {
                client.Client.GetStream().WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendDisconnected(List<ConnectedClient> connectedClients, Guid id)
        {
            //Create the message
            string message = $"{ResponseCode.DisonnectedUser}|{id}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            ////Send
            foreach (var client in connectedClients)
            {
                client.Client.GetStream().WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendConversationInvite(ConnectedClient accepter, ConnectedClient inviter, string conversationId)
        {
            //Send a message to the accepting client
            string message = $"{ResponseCode.InviteReceived}|{conversationId}|{ResponseMessage.AcceptInviteMessage.Replace("USERNAME", inviter.Username)}|{conversationId}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            //Send
            accepter.Client.GetStream().WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendConversationInviteAccepted(Conversation conversation)
        {
            //Send
            if (conversation.Inviter.Client.Connected && conversation.Accepter.Client.Connected)
            {
                //Send a message to the accepting client
                string message = $"{ResponseCode.AcceptedInvite}|{conversation.Id.ToString()}|{conversation.Accepter.Username}";

                //Encode
                byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

                conversation.Inviter.Client.GetStream().WriteAsync(data, 0, data.Length);

                message = $"{ResponseCode.AcceptedInvite}|{conversation.Id.ToString()}|{conversation.Inviter.Username}";

                data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

                conversation.Accepter.Client.GetStream().WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendConversationMessage(ConnectedClient receiver, ConversationMessage convoMessage)
        {
            //Serialized Convo Message
            string jsonUpdatedClients = JsonConvert.SerializeObject(convoMessage);

            //Send a message to the accepting client
            string message = $"{ResponseCode.ReceivedConversationMessage}|{jsonUpdatedClients}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            //Send
            receiver.Client.GetStream().WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Broadcasts an image
        /// </summary>
        public static async void SendImageMessage(ConnectedClient receiver, ConversationMessage convoMessage, byte[] imageBuffer)
        {
            var stream = receiver.Client.GetStream();
            string json = JsonConvert.SerializeObject(convoMessage);
            string header = $"{ResponseCode.ReceivedConversationImage}|{json}|{imageBuffer.Length}\r\n";

            // Turn header into bytes and send it
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            await stream.WriteAsync(headerBytes, 0, headerBytes.Length).ConfigureAwait(false);

            // Send the raw image bytes
            await stream.WriteAsync(imageBuffer, 0, imageBuffer.Length).ConfigureAwait(false);

            // Send a trailing newline to re-sync
            byte[] newline = Encoding.UTF8.GetBytes("\r\n");
            await stream.WriteAsync(newline, 0, newline.Length).ConfigureAwait(false);
        }

        /// <summary>
        /// Broadcasts a list of connected clients to a requesting client
        /// </summary>
        /// <param name="connectedClients">A list of connected clients</param>
        /// <param name="requestingClient">The client requesting the info</param>
        public static async void SendEndConversation(ConnectedClient receiver, string clientUsernameEnded)
        {
            //Send a message to the accepting client
            string message = $"{ResponseCode.ConversationEnded}|{ResponseMessage.ConversationEndedMessage.Replace("USERNAME", clientUsernameEnded)}";

            //Encode
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);

            //Send
            receiver.Client.GetStream().WriteAsync(data, 0, data.Length);
        }
    }
}
