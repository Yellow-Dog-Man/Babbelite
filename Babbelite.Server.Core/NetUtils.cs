using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace Babbelite.Server.Core
{
    public static class NetUtils
    {
        public static IReadOnlyList<IPAddress> LocalIPs => _cachedLocalIPs.Value;
        public static IEnumerable<IPAddress> FilteredLocalIPs => LocalIPs.Where(a =>
            a.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6 && !a.IsLinkLocalAddress());

        static Lazy<List<IPAddress>> _cachedLocalIPs = new Lazy<List<IPAddress>>(() => GetLocalIPs().ToList(), true);

        // Adjusted from LiteNetLib
        // Simply enumerating the interfaces throws exception on Android
        public static HashSet<IPAddress> GetLocalIPs()
        {
            var ips = new HashSet<IPAddress>();

            bool useFallback = false;

            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    try
                    {
                        if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                            continue;

                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                            if (ip.Address != null)
                                ips.Add(ip.Address);
                    }
                    catch (Exception ex)
                    {
                        useFallback = true;
                    }
                }
            }
            catch (Exception ex)
            {
                useFallback = true;
            }

            //Fallback mode
            if (useFallback)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (IPAddress ip in host.AddressList)
                    if (ip != null)
                        ips.Add(ip);
            }

            return ips;
        }

        // https://en.wikipedia.org/wiki/Link-local_address
        public static bool IsLinkLocalAddress(this IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = ipAddress.GetAddressBytes();
                return bytes[0] == 169 && bytes[1] == 254;
            }

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return ipAddress.IsIPv6LinkLocal;

            return false;
        }
    }
}
