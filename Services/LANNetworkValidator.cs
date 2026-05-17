// ???????????????????????????????????????????????????????????????
//  LANNetworkValidator.cs — Validate LAN-only access
//  ???????????????????????????????????????????????????????????????
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace EmpresaApp.Services
{
    /// <summary>
    /// Validates that database access is restricted to LAN devices
    /// </summary>
    public class LANNetworkValidator
    {
        private static LANNetworkValidator? _instance;
        private static readonly object _lock = new object();

        private readonly List<string> _trustedNetworks = new();
        private readonly List<string> _allowedIPs = new();

        private LANNetworkValidator()
        {
            InitializeTrustedNetworks();
        }

        public static LANNetworkValidator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new LANNetworkValidator();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initialize trusted LAN networks (common private IP ranges)
        /// </summary>
        private void InitializeTrustedNetworks()
        {
            // Common private IP ranges
            _trustedNetworks.AddRange(new[]
            {
                "192.168.",      // Class C private
                "10.",           // Class A private
                "172.16.",       // Class B private part 1
                "172.17.",       // Class B private part 2
                "172.18.",       // Class B private part 3
                "172.19.",       // Class B private part 4
                "172.20.",       // Class B private part 5
                "172.21.",       // Class B private part 6
                "172.22.",       // Class B private part 7
                "172.23.",       // Class B private part 8
                "172.24.",       // Class B private part 9
                "172.25.",       // Class B private part 10
                "172.26.",       // Class B private part 11
                "172.27.",       // Class B private part 12
                "172.28.",       // Class B private part 13
                "172.29.",       // Class B private part 14
                "172.30.",       // Class B private part 15
                "172.31.",       // Class B private part 16
                "127.",          // Localhost
            });
        }

        /// <summary>
        /// Get all local IP addresses
        /// </summary>
        public List<string> GetLocalIPAddresses()
        {
            var ips = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                ips.AddRange(host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting local IPs: {ex.Message}");
            }
            return ips;
        }

        /// <summary>
        /// Check if an IP address is on the trusted LAN
        /// </summary>
        public bool IsIPOnLAN(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out var parsedIP))
                {
                    string ipStr = parsedIP.ToString();
                    return _trustedNetworks.Any(network => ipStr.StartsWith(network));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating IP: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Get the server IP from connection string
        /// </summary>
        public string ExtractServerIP(string connectionString)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                string server = builder.DataSource ?? "";

                // Handle localhost
                if (server == "." || server.Contains("(localdb)") || server == "localhost")
                    return "127.0.0.1";

                // Try to resolve hostname
                try
                {
                    var addresses = Dns.GetHostAddresses(server);
                    if (addresses.Length > 0)
                        return addresses[0].ToString();
                }
                catch { }

                return server;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting server IP: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Validate database server is accessible and on LAN
        /// </summary>
        public bool ValidateDatabaseServer(string connectionString)
        {
            try
            {
                var serverIP = ExtractServerIP(connectionString);

                // Try to connect
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                conn.Open();

                // If it's a remote server, check if it's on LAN
                if (!serverIP.StartsWith("127") && !serverIP.StartsWith("localhost"))
                {
                    if (!IsIPOnLAN(serverIP))
                    {
                        System.Diagnostics.Debug.WriteLine($"Server {serverIP} is not on trusted LAN");
                        return false;
                    }
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = 3;
                cmd.ExecuteScalar();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add trusted IP address
        /// </summary>
        public void AddTrustedIP(string ipAddress)
        {
            if (!_allowedIPs.Contains(ipAddress))
                _allowedIPs.Add(ipAddress);
        }

        /// <summary>
        /// Remove trusted IP address
        /// </summary>
        public void RemoveTrustedIP(string ipAddress)
        {
            _allowedIPs.Remove(ipAddress);
        }

        /// <summary>
        /// Get list of allowed IPs
        /// </summary>
        public List<string> GetTrustedIPs() => new List<string>(_allowedIPs);

        /// <summary>
        /// Check network connectivity
        /// </summary>
        public bool CheckNetworkConnectivity()
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send("8.8.8.8", 1000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get network interface information
        /// </summary>
        public List<string> GetNetworkInterfaces()
        {
            var interfaces = new List<string>();
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var nic in nics)
                {
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                        nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        var ipProps = nic.GetIPProperties();
                        foreach (var ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                interfaces.Add($"{nic.Name}: {ip.Address}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting network interfaces: {ex.Message}");
            }
            return interfaces;
        }
    }
}
