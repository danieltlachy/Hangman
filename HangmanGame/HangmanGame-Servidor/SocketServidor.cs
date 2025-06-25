using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HangmanGame_Servidor
{
    public class SocketServidor
    {
        private TcpListener listener;
        private ConcurrentDictionary<string, List<TcpClient>> clientes;
        private bool isRunning;

        public SocketServidor(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            clientes = new ConcurrentDictionary<string, List<TcpClient>>();
            isRunning = false;
        }

        public async Task StartAsync()
        {
            listener.Start();
            isRunning = true;
            Console.WriteLine($"Servidor de sockets iniciado en el puerto {((IPEndPoint)listener.LocalEndpoint).Port}");

            while (isRunning)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client); // Manejar el cliente en segundo plano
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al aceptar cliente: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string codigoPartida = "lobby"; // Valor por defecto para clientes en ListaPartidasDisponibles
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Registrar el cliente en el "lobby" al conectarse
                clientes.AddOrUpdate("lobby", new List<TcpClient> { client }, (key, list) =>
                {
                    lock (list)
                    {
                        list.Add(client);
                    }
                    return list;
                });
                Console.WriteLine($"Cliente registrado en el lobby");

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"Mensaje recibido: {message}");

                    if (message == "MONITOR_ESTADISTICAS")
                    {
                        Console.WriteLine("Cliente registrado para monitoreo de estadísticas en el lobby.");
                        // No necesitamos moverlo a otra partida, se queda en "lobby"
                    }

                    // Actualizar codigoPartida si el mensaje contiene un código específico
                    if (message.Contains(":"))
                    {
                        var partes = message.Split(':');
                        if (partes.Length > 1)
                        {
                            string nuevoCodigo = partes[1];
                            if (nuevoCodigo != codigoPartida) // Evitar duplicados
                            {
                                // Mover el cliente a la nueva partida
                                lock (clientes)
                                {
                                    if (clientes.TryGetValue(codigoPartida, out var listaClientes))
                                    {
                                        lock (listaClientes)
                                        {
                                            listaClientes.Remove(client);
                                            if (!listaClientes.Any())
                                            {
                                                clientes.TryRemove(codigoPartida, out _);
                                            }
                                        }
                                    }
                                    codigoPartida = nuevoCodigo;
                                    clientes.AddOrUpdate(codigoPartida, new List<TcpClient> { client }, (key, list) =>
                                    {
                                        lock (list)
                                        {
                                            list.Add(client);
                                        }
                                        return list;
                                    });
                                    Console.WriteLine($"Cliente actualizado a la partida {codigoPartida}");
                                }
                            }
                        }
                    }

                    // Procesar el mensaje recibido
                    if (message.StartsWith("INICIAR_PARTIDA:") || message.StartsWith("UNION_RETADOR:") ||
                        message.StartsWith("ACTUALIZAR_PARTIDA:") || message.StartsWith("FIN_PARTIDA:") ||
                        message.StartsWith("PARTIDA_CANCELADA:"))
                    {
                        var partes = message.Split(':');
                        if (partes.Length >= 2)
                        {
                            string codigo = partes[1];
                            await NotificarClientes(codigo, message);
                        }
                    }
                    else
                    {
                        if (message.StartsWith("NUEVA_PARTIDA:"))
                        {
                            // Enviar a todos los clientes, incluidos los del "lobby"
                            await BroadcastToAll(message);
                        }

                        if (message.StartsWith("PARTIDA_DESECHADA:"))
                        {
                            // Enviar a todos los clientes, incluidos los del "lobby"
                            await BroadcastToAll(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al manejar cliente: {ex.Message}");
            }
            finally
            {
                // Eliminar el cliente de su partida o lobby
                if (codigoPartida != null && clientes.TryGetValue(codigoPartida, out var listaClientes))
                {
                    lock (listaClientes)
                    {
                        listaClientes.Remove(client);
                        if (!listaClientes.Any())
                        {
                            clientes.TryRemove(codigoPartida, out _);
                        }
                    }
                }
                client.Close();
            }
        }

        private async Task NotificarClientes(string codigoPartida, string mensaje)
        {
            if (clientes.TryGetValue(codigoPartida, out var clientesPartida))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(mensaje);
                var clientesActivos = new List<TcpClient>(); // Lista temporal para clientes activos

                lock (clientesPartida)
                {
                    clientesActivos.AddRange(clientesPartida.Where(c => c.Connected));
                }

                foreach (var cliente in clientesActivos)
                {
                    try
                    {
                        NetworkStream stream = cliente.GetStream();
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        await stream.FlushAsync();
                        Console.WriteLine($"Mensaje enviado a cliente: {mensaje}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al enviar mensaje a cliente: {ex.Message}");
                        lock (clientesPartida)
                        {
                            clientesPartida.Remove(cliente);
                        }
                    }
                }

                // Limpiar clientes desconectados
                lock (clientesPartida)
                {
                    if (!clientesPartida.Any())
                    {
                        clientes.TryRemove(codigoPartida, out _);
                    }
                }
            }
            else
            {
                Console.WriteLine($"No hay clientes registrados para la partida {codigoPartida}");
            }
        }

        public async Task NotificarUnionRetador(string codigoPartida, string nicknameRetador)
        {
            if (clientes.TryGetValue(codigoPartida, out List<TcpClient> clientList))
            {
                string message = $"UNION_RETADOR:{nicknameRetador}";
                byte[] data = Encoding.UTF8.GetBytes(message);

                foreach (var client in clientList.ToArray()) // Usar ToArray para evitar problemas de concurrencia
                {
                    if (client.Connected)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            await stream.WriteAsync(data, 0, data.Length);
                            Console.WriteLine($"Notificación enviada a {codigoPartida}: {message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al notificar a cliente: {ex.Message}");
                            clientList.Remove(client);
                        }
                    }
                }

                if (clientList.Count == 0)
                    clientes.TryRemove(codigoPartida, out _);
            }
        }

        private async Task BroadcastToAll(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var allClients = new List<TcpClient>();

            lock (clientes)
            {
                foreach (var clientList in clientes.Values)
                {
                    lock (clientList)
                    {
                        allClients.AddRange(clientList.Where(c => c.Connected));
                    }
                }
            }

            foreach (var cliente in allClients)
            {
                try
                {
                    NetworkStream stream = cliente.GetStream();
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();
                    Console.WriteLine($"Mensaje enviado a cliente: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al enviar mensaje a cliente: {ex.Message}");
                    lock (clientes)
                    {
                        foreach (var clientList in clientes.Values)
                        {
                            lock (clientList)
                            {
                                clientList.Remove(cliente);
                            }
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            foreach (var clientList in clientes.Values)
            {
                foreach (var client in clientList)
                {
                    client.Close();
                }
            }
            listener.Stop();
        }
    }
}
