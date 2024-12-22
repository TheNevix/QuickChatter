using System.Net.Sockets;
using System.Net;

namespace QuickChatter.Client.Helpers
{
    public static class IpHelpers
    {
        public static string GetLocalIPAddress()
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
                     ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211))
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
