using QuickChatter.Models;
using QuickChatter.Models.Settings;
using QuickChatter.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    //List of connected clients
    private static List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();
    private static List<Conversation> ClientConversations = new List<Conversation>();

    static async Task Main(string[] args)
    {
        //Create a new TcpListener to listen to clients that want to connect
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("TCP Server started on port 5000.");

        while (true)
        {
            //Client wants to connect
            TcpClient client = await listener.AcceptTcpClientAsync();

            //Log that a new client was connected
            Console.WriteLine("Client connected.");

            //Add the client list
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        try
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                //Read the data received from the client {username,ip}
                string receivedData = await reader.ReadLineAsync();

                //If null, destroy connection
                if (receivedData == null) break; // Client disconnected

                //Log what we received
                Console.WriteLine($"Received: {receivedData}");

                //Parse the received data {username,ip}
                var parts = receivedData.Split('|');

                //Check if we have exactly 2 parts
                if (parts[0] == RequestCode.Connect)
                {
                    //Handle Connect
                    HandleConnect(client, parts);
                }
                else if (parts[0] == RequestCode.Disconnect)
                {
                    //Handle Connect
                    HandleDisconnect(client, parts);
                    break;
                }
                else if (parts[0] == RequestCode.InviteForConversation)
                {
                    HandleInvite(client, parts);
                }
                else if (parts[0] == RequestCode.AcceptConversationInvite)
                {
                    HandleInviteAccept(client, parts);
                }
                else if (parts[0] == RequestCode.SendConversationMessage)
                {
                    HandleConversationMessage(client, parts);
                }
                else if (parts[0] == RequestCode.EndConversation)
                {
                    HandleEndConversation(client, parts);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client handling error: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    public static async Task HandleConnect(TcpClient client, string[] parts)
    {
        //Add the new Client
        ConnectedClients.Add(new ConnectedClient { Id = Guid.NewGuid(), Client = client, IsAvailable = true });

        //Get the username
        string username = parts[1];

        //Get the ip
        string ip = parts[2];

        //Find the client index in the list of connected clients
        var foundClientIndex = ConnectedClients.FindIndex(cc => cc.Client == client);

        //Update their info
        ConnectedClients[foundClientIndex].Ip = ip;
        ConnectedClients[foundClientIndex].Username = username;

        //Send the new client the info of all the connected users
        Broadcaster.SendListOfConnectedUsers(ConnectedClients, ConnectedClients[foundClientIndex]);

        //Send new added user to everyone
        Broadcaster.SendNewOnlineUser(ConnectedClients, ConnectedClients[foundClientIndex]);
    }

    public static async Task HandleDisconnect(TcpClient client, string[] parts)
    {
        // Find the client to remove
        var clientToRemove = ConnectedClients.FirstOrDefault(cc => cc.Client == client);
        if (clientToRemove == null)
            return;

        // Remove the client from the list
        ConnectedClients.Remove(clientToRemove);

        // Notify all remaining clients that this user went offline
        Broadcaster.SendDisconnected(ConnectedClients, clientToRemove.Id);

        // Optionally, close the connection
        try
        {
            if (client != null && client.Client != null && client.Connected)
            {
                client.GetStream()?.Close(); // Optional: close the stream explicitly
                client.Close();
            }
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, no action needed
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions if needed
            Console.WriteLine($"Error while closing client: {ex.Message}");
        }

        // Log or debug
        Console.WriteLine($"Client {clientToRemove.Username} ({clientToRemove.Ip}) disconnected.");
    }

    public static async Task HandleInvite(TcpClient client, string[] parts)
    {
        var accepterIndex = ConnectedClients.FindIndex(cc => cc.Username == parts[1]);
        var inviterIndex = ConnectedClients.FindIndex(cc => cc.Client == client);

        Guid conversationId = Guid.NewGuid();

        //Add them to the conversation list
        ClientConversations.Add(new Conversation
        {
            Id = conversationId,
            Accepter = ConnectedClients[accepterIndex],
            Inviter = ConnectedClients[inviterIndex],
            IsAccepted = false,
            Messages = new List<ConversationMessage>()
        });

        //Update status to unavailable
        ConnectedClients[accepterIndex].IsAvailable = false;
        ConnectedClients[inviterIndex].IsAvailable = false;

        //Send invite for accepter to accept
        Broadcaster.SendConversationInvite(ConnectedClients[accepterIndex], ConnectedClients[inviterIndex], conversationId.ToString());

        //Send updated status to everyone
        Broadcaster.SendUpdatedClients(ConnectedClients, new List<ConnectedClient> { ConnectedClients[accepterIndex], ConnectedClients[inviterIndex] });
    }

    public static async Task HandleInviteAccept(TcpClient client, string[] parts)
    {
        var conversationIndex = ClientConversations.FindIndex(cc => cc.Id.ToString() == parts[1]);

        ClientConversations[conversationIndex].IsAccepted = true;

        //Send invite for accepter to accept
        Broadcaster.SendConversationInviteAccepted(ClientConversations[conversationIndex]);
    }

    public static async Task HandleConversationMessage(TcpClient client, string[] parts)
    {
        ConnectedClient sender = new ConnectedClient();
        ConnectedClient receiver = new ConnectedClient();
        var conversationIndex = ClientConversations.FindIndex(cc => cc.Id.ToString() == parts[1]);

        //Check who sended it
        if (ClientConversations[conversationIndex].Inviter.Id.ToString() == parts[2])
        {
            sender = ClientConversations[conversationIndex].Inviter;
            receiver = ClientConversations[conversationIndex].Accepter;
        }
        else
        {
            sender = ClientConversations[conversationIndex].Accepter;
            receiver = ClientConversations[conversationIndex].Inviter;
        }

        var convoMessage = new ConversationMessage
        {
            Message = parts[3],
            SentBy = new User
            {
                Id = sender.Id,
                IsAvailable = false,
                Username = sender.Username,
                Ip = sender.Ip,
            },
            SentOn = DateTime.UtcNow,
        };

        //Add the message to the list
        ClientConversations[conversationIndex].Messages.Add(convoMessage);

        //Send the received message to the other client
        Broadcaster.SendConversationMessage(receiver, convoMessage);
    }

    public static async Task HandleEndConversation(TcpClient client, string[] parts)
    {
        //Get the conversation index
        var conversationIndex = ClientConversations.FindIndex(cc => cc.Id.ToString() == parts[1]);

        ConnectedClient clientToNotify = new ConnectedClient();
        ConnectedClient clientRequestedToEnd = new ConnectedClient();

        if (ClientConversations[conversationIndex].Accepter.Id.ToString() == parts[2])
        {
            clientToNotify = ClientConversations[conversationIndex].Inviter;
            clientRequestedToEnd = ClientConversations[conversationIndex].Accepter;
        }
        else
        {
            clientToNotify = ClientConversations[conversationIndex].Accepter;
            clientRequestedToEnd = ClientConversations[conversationIndex].Inviter;
        }

        //To the other client that the other one ended the conversation
        Broadcaster.SendEndConversation(clientToNotify, clientRequestedToEnd.Username);

        //Remove the conversation from the list
        ClientConversations.RemoveAt(conversationIndex);

        var clientToNotifyIndex = ConnectedClients.FindIndex(cc => cc.Id == clientToNotify.Id);
        var clientRequestedToEndIndex = ConnectedClients.FindIndex(cc => cc.Id == clientRequestedToEnd.Id);

        ConnectedClients[clientToNotifyIndex].IsAvailable = true;
        ConnectedClients[clientRequestedToEndIndex].IsAvailable = true;

        Broadcaster.SendUpdatedClients(ConnectedClients, new List<ConnectedClient> { ConnectedClients[clientToNotifyIndex], ConnectedClients[clientRequestedToEndIndex] });
    }
}

