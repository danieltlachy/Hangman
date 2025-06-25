using System;
using Biblioteca.Servicios;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace HangmanGame_Servidor
{
    internal class Program
    {
        private static SocketServidor socketServidor;

        static async Task Main(string[] args)
        {
            // Iniciar el servidor de sockets en un puerto diferente (12345)
            socketServidor = new SocketServidor(12345);
            Task socketTask = socketServidor.StartAsync();

            // Iniciar el host de WCF
            using (ServiceHost host = new ServiceHost(typeof(HangmanServicio)))
            {
                // Configurar el endpoint principal
                host.AddServiceEndpoint(
                    typeof(IHangmanService),
                    new NetTcpBinding { Security = { Mode = SecurityMode.None } },
                    "net.tcp://localhost:8001/HangmanService");

                // Habilitar metadatos
                ServiceMetadataBehavior metadataBehavior = new ServiceMetadataBehavior();
                metadataBehavior.HttpGetEnabled = false; // No usamos HTTP
                metadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(metadataBehavior);

                // Agregar un endpoint para metadatos (usando mex)
                host.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexTcpBinding(),
                    "net.tcp://localhost:8001/HangmanService/mex");

                try
                {
                    host.Open();
                    Console.WriteLine("Servicio WCF iniciado en net.tcp://localhost:8001/HangmanService");
                    Console.WriteLine("Metadatos disponibles en net.tcp://localhost:8001/HangmanService/mex");
                    Console.WriteLine("Servidor de sockets iniciado en puerto 12345");
                    Console.WriteLine("Presiona cualquier tecla para detener el servidor...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al iniciar el servicio: {ex.Message}");
                }
                finally
                {
                    // Detener el servidor de sockets y cerrar el host de WCF
                    socketServidor.Stop();
                    host.Close();
                    Console.WriteLine("Servicio WCF detenido.");
                    Console.WriteLine("Servidor de sockets detenido.");
                    await socketTask;
                }
            }
        }

        // Método estático para que HangmanServicio acceda al servidor de sockets
        public static async Task NotificarUnionRetador(string codigoPartida, string nicknameRetador)
        {
            await socketServidor.NotificarUnionRetador(codigoPartida, nicknameRetador);
        }
    }
}