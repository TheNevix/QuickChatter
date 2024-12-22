using Microsoft.Extensions.DependencyInjection;
using QuickChatter.Models;
using QuickChatter.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    //List of connected clients
    private static List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();

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
            //Add the new Client
            ConnectedClients.Add(new ConnectedClient { Client = client, IsAvailable = true });

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
                var parts = receivedData.Split(',');

                //Check if we have exactly 2 parts
                if (parts.Length == 2)
                {
                    //Get the username
                    string username = parts[0];

                    //Get the ip
                    string ip = parts[1];

                    //Find the client index in the list of connected clients
                    var foundClientIndex = ConnectedClients.FindIndex(cc => cc.Client == client);

                    //Update their info
                    ConnectedClients[foundClientIndex].Ip = ip;
                    ConnectedClients[foundClientIndex].Username = username;

                    //Send the new client the info of all the connected users
                    Broadcaster.SendListOfConnectedUsers(ConnectedClients, ConnectedClients[foundClientIndex]);
                    
                    //Send all clients info about the newly added client
                    //BroadcastUserCount();
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
}
