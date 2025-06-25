using System;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HangmanGame_Cliente
{
    public class SocketCliente
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isListening;
        private CancellationTokenSource cts;
        public event Action<string> ConnectionLost;
        public event Action<string> MessageReceived;
        public bool IsConnected => client?.Connected ?? false;

        public async Task ConectarAsync(string ip, int port)
        {
            client = new TcpClient();
            try
            {
                await client.ConnectAsync(ip, port);
                stream = client.GetStream();
                isListening = true;
                cts = new CancellationTokenSource();
                _ = Task.Run(() => ListenAsync(cts.Token)); // Iniciar escucha pasiva
            }
            catch(Exception ex)
            {
                OnConnectionLost(ex.Message);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (client?.Connected ?? false)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();
                    Console.WriteLine($"SocketCliente: Mensaje enviado desde cliente: {message}");
                }
                catch (Exception ex)
                {
                    OnConnectionLost($"Error al enviar mensaje: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("SocketCliente: No se puede enviar mensaje, cliente no conectado.");
            }
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (isListening && client?.Connected == true && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"SocketCliente: Mensaje recibido en cliente: {mensaje}");
                        MessageReceived?.Invoke(mensaje);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("SocketCliente: Escucha cancelada correctamente.");
            }
            catch (Exception ex)
            {
                OnConnectionLost($"Error al enviar mensaje: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("SocketCliente: Escucha finalizada.");
                stream?.Dispose();
            }
        }

        public void Desconectar()
        {
            /*Console.WriteLine("SocketCliente: Desconectando...");
            isListening = false;
            cts?.Cancel();
            stream?.Dispose();
            client?.Close();
            cts?.Dispose();*/

            Console.WriteLine("SocketCliente: Desconectando...");
            isListening = false; // Detener la escucha
            cts?.Cancel(); // Cancelar la tarea de escucha
            stream?.Close();
            client?.Close();
            cts?.Dispose(); // Liberar cts
            cts = null;
            Console.WriteLine("SocketCliente: Escucha finalizada.");
        }

        private void OnConnectionLost(string message)
        {
            ConnectionLost?.Invoke(message);
            Console.WriteLine($"Conexión perdida: {message}");
        }
    }
}
