using Newtonsoft.Json;
using QuickChatter.Models;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace QuickChatter.Client.Helpers
{
    public static class ServerHelper
    {
        /// <summary>
        /// Helper method to connect to the server
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> ConnectToServer(TcpClient client, StreamWriter writer, string username)
        {
            try
            {
                //Get the Ip of the client
                string userIp = IpHelpers.GetLocalIPAddress();

                //Check if null
                if (string.IsNullOrEmpty(userIp))
                {
                    return false;
                }

                //Connect to the server
                await writer.WriteLineAsync($"{username},{userIp}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task ListenForUpdates(TcpClient client)
        {
            try
            {
                using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                while (true)
                {
                    string message = await reader.ReadLineAsync();
                    if (message == null)
                    {
                        break;
                    }
                    else
                    {
                        var users = JsonConvert.DeserializeObject<List<User>>(message);
                        var e = 5;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }
    }
}
