using Biblioteca.DTO;
using HangmanGame_Cliente.HangmanServicioReferencia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace HangmanGame_Cliente.Cliente.Vistas
{
    public partial class ListaPartidasDisponibles : Page
    {
        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private JugadorDTO jugadorDTO;
        private bool isInitialized;
        private string idioma;
        public event EventHandler<string> IdiomaSeleccionado;

        public ListaPartidasDisponibles()
        {
            InitializeComponent();
            cliente = new HangmanServiceClient();
            CargarPartidas();
            Loaded += ListaPartidasDisponibles_Loaded;
            Unloaded += ListaPartidasDisponibles_Unloaded;
        }

        private async void ListaPartidasDisponibles_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return; 
            isInitialized = true;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                MessageBox.Show("Error: No se encontró MainWindow.");
                return;
            }
            jugadorDTO = mainWindow.GetJugadorAutenticado();
            if (jugadorDTO == null)
            {
                MessageBox.Show("Error: No se encontró el jugador autenticado. MainWindow HashCode: " + mainWindow.GetHashCode());
                mainWindow.CambiarPagina(new IniciarSesion());
                return;
            }

            socketCliente = new SocketCliente();
            socketCliente.ConnectionLost += OnConnectionLost;
            socketCliente.MessageReceived += OnMessageReceived;

            try
            {
                await socketCliente.ConectarAsync("127.0.0.1", 12345);
                await socketCliente.SendMessageAsync("REGISTRO_LOBBY");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar al servidor de sockets: " + ex.Message);
                OnConnectionLost(ex.Message);
            }

            CargarPartidas();
        }

        private void ListaPartidasDisponibles_Unloaded(object sender, RoutedEventArgs e)
        {
            socketCliente?.Desconectar();
            try { cliente.Close(); } catch { cliente.Abort(); }
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("NUEVA_PARTIDA:") || 
                message.StartsWith("PARTIDA_DESECHADA:"))
            {
                var partes = message.Split(':');
                if (partes.Length >= 2)
                {
                    string codigo = partes[1];

                    Dispatcher.Invoke(() =>
                    {
                        CargarPartidas(); 
                    });
                }
            }
        }

        private void Unirse(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PartidaDTO partida)
            {
                try
                {
                    int idJugador = jugadorDTO.id_jugador;
                    ResponsePartidaDTO response = cliente.UnirsePartida(partida.Codigo, idJugador);

                    if (response != null && response.success)
                    {
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow?.CambiarPagina(new SalaDeEspera(partida.Codigo));
                    }
                    else
                    {
                        MessageBox.Show($"Error al unirse: {response?.message ?? "Sin mensaje"}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al unirse: {ex.Message}");
                }
            }
        }

        private void GenerarPartida(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CambiarPagina(new FormularioPartida());
        }

        private void CargarPartidas()
        {
            try
            {
                ResponsePartidaDTO response = cliente.ObtenerPartidasDisponibles();

                if (response != null && response.success && response.data != null)
                {
                    var partidasDTO = response.data;
                    if (partidasDTO != null && partidasDTO.Partidas != null)
                    {
                        foreach (var partida in partidasDTO.Partidas)
                        {
                            partida.FechaPartida = partida.fecha.ToString("yyyy-MM-dd");
                        }
                        listViewPartidas.ItemsSource = partidasDTO.Partidas;
                    }
                    else
                    {
                        listViewPartidas.ItemsSource = new List<PartidaDTO> { new PartidaDTO { Nickname = "No hay partidas disponibles" } };
                    }
                }
                else
                {
                    MessageBox.Show($"Error al cargar partidas: {response?.message ?? "Sin mensaje"}");
                    listViewPartidas.ItemsSource = new List<PartidaDTO> { new PartidaDTO { Nickname = "Error al cargar" } };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                listViewPartidas.ItemsSource = new List<PartidaDTO> { new PartidaDTO { Nickname = "Error al cargar" } };
            }
        }

        private void VerEstadisticas(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CambiarPagina(new Estadisticas());
        }

        private void OnConnectionLost(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.HandleConnectionLost(message ?? "Se ha perdido la conexión con el servidor.");
            });
        }

        private void btnCambiarIdioma_Click(object sender, RoutedEventArgs e)
        {
            IdiomaHelper.AlternarIdioma();
            IdiomaSeleccionado?.Invoke(this, IdiomaHelper.IdiomaActual == "en" ? "Ingles" : "Spanish");
        }

        private void ModificarPerfil(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CambiarPagina(new FormularioUsuario(true));
        }
    }
}
