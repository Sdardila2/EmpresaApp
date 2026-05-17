// ???????????????????????????????????????????????????????????????
//  ConfigManager.cs — Gestor de configuración robusto
//  Persiste y valida configuración de red con caché
// ???????????????????????????????????????????????????????????????
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EmpresaApp.Utils;

namespace EmpresaApp.Data
{
    /// <summary>
    /// Configuración guardada (serializable)
    /// </summary>
    public class NetworkConfig
    {
        public string? ConnectionString { get; set; }
        public string? ServerAddress { get; set; }
        public string? DatabaseName { get; set; }
        public DateTime LastValidated { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Gestor avanzado de configuración de red
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigDirectory = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");

        private static readonly string ConfigFilePath = 
            Path.Combine(ConfigDirectory, "network.json");

        private static readonly string LegacyConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "servers.cfg");

        private static NetworkConfig? _cachedConfig;

        static ConfigManager()
        {
            EnsureConfigDirectory();
            MigrateFromLegacy();
        }

        /// <summary>
        /// Asegura que el directorio de configuración existe
        /// </summary>
        private static void EnsureConfigDirectory()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating config directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Migra desde el archivo legacy servers.cfg
        /// </summary>
        private static void MigrateFromLegacy()
        {
            try
            {
                if (File.Exists(LegacyConfigPath) && !File.Exists(ConfigFilePath))
                {
                    string legacyConnStr = File.ReadAllText(LegacyConfigPath).Trim();
                    if (!string.IsNullOrEmpty(legacyConnStr))
                    {
                        var config = new NetworkConfig
                        {
                            ConnectionString = legacyConnStr,
                            ServerAddress = ExtractServerFromConnStr(legacyConnStr),
                            DatabaseName = ExtractDatabaseFromConnStr(legacyConnStr),
                            LastValidated = DateTime.Now,
                            IsValid = true
                        };
                        SaveConfig(config);
                    }

                    // Eliminar archivo legacy después de migración exitosa
                    try { File.Delete(LegacyConfigPath); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error migrating legacy config: {ex.Message}");
            }
        }

        /// <summary>
        /// Extrae servidor de la cadena de conexión
        /// </summary>
        private static string ExtractServerFromConnStr(string connStr)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
                return builder.DataSource ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Extrae base de datos de la cadena de conexión
        /// </summary>
        private static string ExtractDatabaseFromConnStr(string connStr)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
                return builder.InitialCatalog ?? "EmpresaApp";
            }
            catch
            {
                return "EmpresaApp";
            }
        }

        /// <summary>
        /// Guarda la configuración en JSON
        /// </summary>
        public static void SaveConfig(NetworkConfig config)
        {
            try
            {
                EnsureConfigDirectory();
                config.LastValidated = DateTime.Now;

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                File.WriteAllText(ConfigFilePath, json);
                _cachedConfig = config;

                System.Diagnostics.Debug.WriteLine($"? Config saved to {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error saving config: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga la configuración desde JSON
        /// </summary>
        public static NetworkConfig? LoadConfig()
        {
            try
            {
                // Retornar del caché si existe
                if (_cachedConfig != null)
                    return _cachedConfig;

                if (!File.Exists(ConfigFilePath))
                    return null;

                string json = File.ReadAllText(ConfigFilePath);
                _cachedConfig = JsonSerializer.Deserialize<NetworkConfig>(json);

                System.Diagnostics.Debug.WriteLine($"? Config loaded from {ConfigFilePath}");
                return _cachedConfig;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error loading config: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Elimina la configuración guardada
        /// </summary>
        public static void DeleteConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                    File.Delete(ConfigFilePath);

                _cachedConfig = null;
                System.Diagnostics.Debug.WriteLine($"? Config deleted");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error deleting config: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si hay configuración guardada
        /// </summary>
        public static bool HasConfig()
        {
            return LoadConfig() != null;
        }

        /// <summary>
        /// Valida la configuración contra el servidor
        /// </summary>
        public static bool ValidateConfig(NetworkConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.ConnectionString))
                    return false;

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(config.ConnectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                cmd.CommandTimeout = 5;

                int result = (int)cmd.ExecuteScalar()!;
                bool isValid = result > 0;

                config.IsValid = isValid;
                config.LastValidated = DateTime.Now;

                if (isValid)
                {
                    SaveConfig(config);
                    System.Diagnostics.Debug.WriteLine($"? Config validated successfully");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Config validation failed: {ex.Message}");
                config.IsValid = false;
                return false;
            }
        }

        /// <summary>
        /// Obtiene el historial de configuraciones
        /// </summary>
        public static List<NetworkConfig> GetConfigHistory()
        {
            var history = new List<NetworkConfig>();
            var config = LoadConfig();
            if (config != null)
                history.Add(config);
            return history;
        }

        /// <summary>
        /// Limpia la caché de configuración
        /// </summary>
        public static void ClearCache()
        {
            _cachedConfig = null;
        }
    }
}
