using Biblioteca.DTO;
using HangmanGame_Cliente.HangmanServicioReferencia;
using HangmanGame_Cliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HangmanGame_Cliente.Cliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioPartida.xaml
    /// </summary>
    public partial class FormularioPartida : Page
    {

        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private string categoriaSeleccionada;
        private List<PalabraDTO> palabrasCargadas;
        JugadorDTO jugadorDTO;
        private bool isInitialized;

        public FormularioPartida()
        {
            InitializeComponent();
            cliente = new HangmanServiceClient();
            socketCliente = new SocketCliente();
            Loaded += FormularioPartida_Loaded;
        }

        private void FormularioPartida_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return; // Evitar inicialización múltiple
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

            Task.Run(async () =>
            {
                try
                {
                    await socketCliente.ConectarAsync("127.0.0.1", 12345);
                    socketCliente.ConnectionLost += OnConnectionLost;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error al conectar con el servidor de sockets: {ex.Message}");
                        var mw = Window.GetWindow(this) as MainWindow;
                        mw?.CambiarPagina(new ListaPartidasDisponibles());
                    });
                }
            });
        }

        private void CategoriaMusica(object sender, RoutedEventArgs e)
        {
            categoriaSeleccionada = "Música";
            CargarPalabras();
        }

        private void Modificar(object sender, RoutedEventArgs e)
        {

        }

        private void CategoriaSeries(object sender, RoutedEventArgs e)
        {
            categoriaSeleccionada = "Series";
            CargarPalabras();
        }

        private void CategoriaPeliculas(object sender, RoutedEventArgs e)
        {
            categoriaSeleccionada = "Películas";
            CargarPalabras();
        }

        private void CargarPalabras()
        {
            try
            {
                ResponsePalabrasConIdDTO response = cliente.ObtenerPalabrasPorCategoria(categoriaSeleccionada);

                if (response != null && response.success && response.data != null)
                {
                    var palabrasDTO = response.data;
                    if (palabrasDTO != null && palabrasDTO.PalabrasConId != null)
                    {
                        palabrasCargadas = cliente.ObtenerPalabrasPorCategoria(categoriaSeleccionada)
                            .data?.PalabrasConId ?? new List<PalabraDTO>();
                        listViewPalabras.ItemsSource = palabrasCargadas.Select(p => p.nombre).ToList();
                    }
                    else
                    {
                        listViewPalabras.ItemsSource = new List<string> { "No hay palabras disponibles" };
                    }
                }
                else
                {
                    MessageBox.Show($"Error al cargar palabras: {response?.message ?? "Sin mensaje"}");
                    listViewPalabras.ItemsSource = new List<string> { "Error al cargar" };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                listViewPalabras.ItemsSource = new List<string> { "Error al cargar" };
            }
        }

        private async void SeleccionarPalabra_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && listViewPalabras.SelectedItem is string palabraSeleccionada)
            {
                var palabra = palabrasCargadas.FirstOrDefault(p => p.nombre == palabraSeleccionada);
                if (palabra == null)
                {
                    MessageBox.Show("Error: No se encontró el ID de la palabra seleccionada.");
                    return;
                }

                string codigo = GenerarCodigoUnico();
                int anfitrionId = jugadorDTO.id_jugador; 
                int idIdiomaPartida = 1; 

                ResponsePartidaDTO response = cliente.CrearPartida(anfitrionId, palabra.IdPalabra, codigo, idIdiomaPartida);

                if (response != null && response.success && response.data != null)
                {
                    var partidaDTO = response.data;
                    if (partidaDTO != null)
                    {
                        if (socketCliente != null && socketCliente.IsConnected)
                        {
                            string mensaje = $"NUEVA_PARTIDA:";
                            await socketCliente.SendMessageAsync(mensaje);
                        }
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow?.CambiarPagina(new SalaDeEspera(partidaDTO.Codigo));
                    }
                }
                else
                {
                    MessageBox.Show($"Error al crear la partida: {response?.message ?? "Sin mensaje"}");
                }
            }
        }

        private string GenerarCodigoUnico()
        {
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

        private void CancelarCreacionPartida(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            socketCliente.Desconectar();
            try { cliente.Close(); } catch { cliente.Abort(); }
            mainWindow?.CambiarPagina(new ListaPartidasDisponibles());
        }

        private void OnConnectionLost(string message)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            MessageBox.Show("Error: " + message);
            mainWindow?.HandleConnectionLost(message ?? "Se ha perdido la conexión con el servidor.");
        }

    }
}
