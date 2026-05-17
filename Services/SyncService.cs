// ???????????????????????????????????????????????????????????????
//  SyncService.cs — Real-time synchronization service for LAN
//  ???????????????????????????????????????????????????????????????
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmpresaApp.Data;
using EmpresaApp.Models;

namespace EmpresaApp.Services
{
    /// <summary>
    /// Enumeration of sync events for real-time updates
    /// </summary>
    public enum SyncEventType
    {
        MensajeNuevo,
        MensajeLeido,
        TareaCompletada,
        AsistenciaEntrada,
        AsistenciaSalida,
        ReporteEnviado,
        UsuarioActualizado,
        NotificacionNueva,
        ConexionPerdida,
        ConexionRestaurada
    }

    /// <summary>
    /// Represents a sync event with metadata
    /// </summary>
    public class SyncEvent
    {
        public SyncEventType Tipo { get; set; }
        public string IdEntidad { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Descripcion { get; set; } = "";
        public object? Datos { get; set; }
    }

    /// <summary>
    /// Service for real-time database synchronization on LAN
    /// </summary>
    public class SyncService
    {
        private static SyncService? _instance;
        private static readonly object _lock = new object();

        private readonly System.Threading.Timer _syncTimer;
        private readonly List<Action<SyncEvent>> _subscribers = new();
        private bool _isConnected = true;
        private DateTime _lastSync = DateTime.Now;
        private const int SYNC_INTERVAL_MS = 2000; // 2 segundos
        private const int CONNECTION_CHECK_INTERVAL_MS = 5000; // 5 segundos

        private SyncService()
        {
            _syncTimer = new System.Threading.Timer(PerformSync, null, SYNC_INTERVAL_MS, SYNC_INTERVAL_MS);
        }

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static SyncService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SyncService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Subscribe to sync events
        /// </summary>
        public void Subscribe(Action<SyncEvent> handler)
        {
            lock (_subscribers)
            {
                _subscribers.Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribe from sync events
        /// </summary>
        public void Unsubscribe(Action<SyncEvent> handler)
        {
            lock (_subscribers)
            {
                _subscribers.Remove(handler);
            }
        }

        /// <summary>
        /// Broadcast sync event to all subscribers
        /// </summary>
        private void BroadcastEvent(SyncEvent syncEvent)
        {
            List<Action<SyncEvent>> handlersCopy;
            lock (_subscribers)
            {
                handlersCopy = new List<Action<SyncEvent>>(_subscribers);
            }

            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler.Invoke(syncEvent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in sync handler: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Check connection status to database
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Perform background sync
        /// </summary>
        private void PerformSync(object? state)
        {
            try
            {
                if (!VerifyDatabaseConnection())
                {
                    if (_isConnected)
                    {
                        _isConnected = false;
                        BroadcastEvent(new SyncEvent
                        {
                            Tipo = SyncEventType.ConexionPerdida,
                            Descripcion = "Conexión a la base de datos perdida"
                        });
                    }
                    return;
                }

                if (!_isConnected)
                {
                    _isConnected = true;
                    BroadcastEvent(new SyncEvent
                    {
                        Tipo = SyncEventType.ConexionRestaurada,
                        Descripcion = "Conexión a la base de datos restaurada"
                    });
                }

                _lastSync = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Verify database connection
        /// </summary>
        private bool VerifyDatabaseConnection()
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(DbConfig.ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = 3;
                return cmd.ExecuteScalar() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Notify new message
        /// </summary>
        public void NotifyMensajeNuevo(Mensaje mensaje)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.MensajeNuevo,
                IdEntidad = mensaje.Id,
                Descripcion = $"Nuevo mensaje de {mensaje.RemitenteNombre}",
                Datos = mensaje
            });
        }

        /// <summary>
        /// Notify message read
        /// </summary>
        public void NotifyMensajeLeido(string mensajeId)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.MensajeLeido,
                IdEntidad = mensajeId,
                Descripcion = "Mensaje marcado como leído"
            });
        }

        /// <summary>
        /// Notify task completion
        /// </summary>
        public void NotifyTareaCompletada(string tareaId)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.TareaCompletada,
                IdEntidad = tareaId,
                Descripcion = "Tarea marcada como completada"
            });
        }

        /// <summary>
        /// Notify entry registration
        /// </summary>
        public void NotifyAsistenciaEntrada(RegistroAsistencia registro)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.AsistenciaEntrada,
                IdEntidad = registro.Id,
                Descripcion = $"Entrada registrada para {registro.UsuarioId}",
                Datos = registro
            });
        }

        /// <summary>
        /// Notify exit registration
        /// </summary>
        public void NotifyAsistenciaSalida(RegistroAsistencia registro)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.AsistenciaSalida,
                IdEntidad = registro.Id,
                Descripcion = $"Salida registrada para {registro.UsuarioId}",
                Datos = registro
            });
        }

        /// <summary>
        /// Notify new report
        /// </summary>
        public void NotifyReporteEnviado(ReporteDiario reporte)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.ReporteEnviado,
                IdEntidad = reporte.Id,
                Descripcion = $"Reporte enviado por {reporte.UsuarioNombre}",
                Datos = reporte
            });
        }

        /// <summary>
        /// Notify new notification
        /// </summary>
        public void NotifyNotificacionNueva(Notificacion notificacion)
        {
            BroadcastEvent(new SyncEvent
            {
                Tipo = SyncEventType.NotificacionNueva,
                IdEntidad = notificacion.Id,
                Descripcion = notificacion.Mensaje,
                Datos = notificacion
            });
        }

        /// <summary>
        /// Stop the sync service
        /// </summary>
        public void Stop()
        {
            _syncTimer?.Dispose();
        }
    }
}
