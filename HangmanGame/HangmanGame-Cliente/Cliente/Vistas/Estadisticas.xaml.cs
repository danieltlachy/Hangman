using Biblioteca.DTO;
using HangmanGame_Cliente.HangmanServicioReferencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HangmanGame_Cliente.Cliente.Vistas
{
    public partial class Estadisticas : Page
    {
        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private bool isInitialized;
        JugadorDTO jugadorDTO;

        public Estadisticas()
        {
            InitializeComponent();
            cliente = new HangmanServiceClient();
            socketCliente = new SocketCliente();
            Loaded += Estadistica_Loaded;
            Unloaded += Estadistica_Unloaded;
        }

        private void Estadistica_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return; // Evitar inicialización múltiple
            isInitialized = true;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                MessageBox.Show("Error: No se encontró MainWindow.");
            }
            jugadorDTO = mainWindow.GetJugadorAutenticado();
            if (jugadorDTO == null)
            {
                MessageBox.Show("Error: No se encontró el jugador autenticado. MainWindow HashCode: " + mainWindow.GetHashCode());
                mainWindow.CambiarPagina(new IniciarSesion());
            }

            Task.Run(async () =>
            {
                try
                {
                    await socketCliente.ConectarAsync("127.0.0.1", 12345);
                    await socketCliente.SendMessageAsync("MONITOR_ESTADISTICAS"); 
                    socketCliente.ConnectionLost += OnConnectionLost;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    Dispatcher.Invoke(() => OnConnectionLost(ex.Message));
                }
            });

            CargarEstadisticas();
        }

        private void Estadistica_Unloaded(object sender, RoutedEventArgs e)
        {
            socketCliente.Desconectar();
            try { cliente.Close(); } catch { cliente.Abort(); }
        }

        private void CargarEstadisticas()
        {
            try
            {
                ResponseEstadisticaDTO response = cliente.ObtenerEstadisticasPorJugador(jugadorDTO.id_jugador);
                if (response != null && response.success && response.data != null)
                {
                    var estadisticasDTO = response.data;
                    if (estadisticasDTO != null && estadisticasDTO.Estadisticas != null)
                    {
                        listViewPartidas.ItemsSource = estadisticasDTO.Estadisticas;

                        int totalPuntos = estadisticasDTO.Estadisticas.Sum(e => e.Puntos);
                        if (IdiomaHelper.IdiomaActual.Equals("es"))
                        {
                            labelPuntosTotales.Content = $"Puntos totales: {totalPuntos} pts";
                        }
                        else
                        {
                            labelPuntosTotales.Content = $"Total points: {totalPuntos} pts";
                        }
                    }
                    else
                    {
                        listViewPartidas.ItemsSource = new List<EstadisticaDTO> { new EstadisticaDTO { Nickname = "No hay partidas disponibles" } };
                        if (IdiomaHelper.IdiomaActual.Equals("es"))
                        {
                            labelPuntosTotales.Content = "Puntos totales: 0 pts";
                        }
                        else
                        {
                            labelPuntosTotales.Content = "Total points: 0 pts";
                        }
                    }
                }
                else
                {
                    listViewPartidas.ItemsSource = new List<EstadisticaDTO> { new EstadisticaDTO { Nickname = "No se pudieron cargar las estadísticas" } };
                    labelPuntosTotales.Content = "Puntos totales: 0 pts";
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error al cargar estadísticas: {ex.Message}");
                    labelPuntosTotales.Content = "Puntos totales: 0 pts";
                });
            }
        }

        private void Regresar(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CambiarPagina(new ListaPartidasDisponibles());
        }

        private void OnConnectionLost(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.HandleConnectionLost(message ?? "Se ha perdido la conexión con el servidor.");
            });
        }

    }
}
