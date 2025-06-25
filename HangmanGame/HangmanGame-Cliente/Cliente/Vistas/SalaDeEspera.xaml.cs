using Biblioteca.DTO;
using HangmanGame_Cliente.Cliente.Alertas;
using HangmanGame_Cliente.HangmanServicioReferencia;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HangmanGame_Cliente.Cliente.Vistas
{

    public partial class SalaDeEspera : Page
    {
        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private string codigoPartida;
        JugadorDTO jugadorDTO;
        private bool isListening;
        private bool isInitialized;
        private int idJugadorActual;

        public SalaDeEspera(string codigoPartida)
        {
            InitializeComponent();
            this.codigoPartida = codigoPartida;
            lbCodigo.Content = codigoPartida;
            cliente = new HangmanServiceClient();
            socketCliente = new SocketCliente();
            isListening = true;
            Loaded += SalaDeEspera_Loaded;
        }

        private void SalaDeEspera_Loaded(object sender, RoutedEventArgs e)
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
            idJugadorActual = jugadorDTO.id_jugador;

            Task.Run(async () =>
            {
                try
                {
                    await socketCliente.ConectarAsync("127.0.0.1", 12345);
                    await socketCliente.SendMessageAsync($"REGISTRO_PARTIDA:{codigoPartida}");
                    socketCliente.MessageReceived += OnMessageReceived;
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

            VerificarEstadoPartida();
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("UNION_RETADOR:"))
                {
                    string nicknameRetador = message.Substring("UNION_RETADOR:".Length);
                    var estadoPartida = cliente.ObtenerEstadoPartida(codigoPartida);
                    bool esAnfitrion = idJugadorActual == estadoPartida.partida.IdAdivinador;
                    if (esAnfitrion)
                    {
                        AjustarTextos(nicknameRetador);

                        btnComenzar.Visibility = Visibility.Visible;
                        lbAvisoPartida.Visibility = Visibility.Visible;
                        lbRetador.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        string nicknameAnfitrion = estadoPartida.partida.Nickname;
                        AjustarTextos(nicknameAnfitrion);

                        lbRetador.Visibility = Visibility.Visible;
                        lbAvisoPartida.Visibility = Visibility.Visible;
                        btnComenzar.Visibility = Visibility.Hidden;
                    }
                }
                else if (message.StartsWith("INICIAR_PARTIDA:"))
                {
                    var partes = message.Split(':');
                    if (partes.Length >= 2 && partes[1] == codigoPartida)
                    {
                        isListening = false;
                        socketCliente.MessageReceived -= OnMessageReceived;

                        // Cerrar recursos antes de navegar
                        socketCliente.Desconectar();
                        try { cliente.Close(); } catch { cliente.Abort(); }

                        // Intentar obtener la ventana principal
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.CambiarPagina(new PartidaAhorcado(codigoPartida));
                        }
                        else
                        {
                            MessageBox.Show($"Error: MainWindow es null en el retador ID {idJugadorActual}. Application.Current.MainWindow: {Application.Current.MainWindow}");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Código recibido {partes[1]} no coincide con {codigoPartida}");
                    }
                }
                else if (message.StartsWith("PARTIDA_DESECHADA:"))
                {
                    var partes = message.Split(':');
                    if (partes.Length >= 2 && partes[1] == codigoPartida)
                    {
                        isListening = false;
                        socketCliente.MessageReceived -= OnMessageReceived;

                        socketCliente.Desconectar();
                        try { cliente.Close(); } catch { cliente.Abort(); }

                        // Intentar obtener la ventana principal
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            MostrarAlertaBloqueante(new PartidaCancelada());
                            mainWindow.CambiarPagina(new ListaPartidasDisponibles());
                        }
                        else
                        {
                            MessageBox.Show($"Error: MainWindow es null en el retador ID {idJugadorActual}. Application.Current.MainWindow: {Application.Current.MainWindow}");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Código recibido {partes[1]} no coincide con {codigoPartida}");
                    }
                }
                else
                {
                    Console.WriteLine($"Mensaje no reconocido: {message}");
                }
            });
        }


        private void AjustarTextos(string nickname)
        {
            if (IdiomaHelper.IdiomaActual.Equals("es"))
            {
                tbAvisoPartida.Text = $"Estás por comenzar una partida con \n{nickname}, ¿estás listo?";
                lbRetador.Content = $"¡{nickname} se ha unido a la partida!";
            }
            else
            {
                tbAvisoPartida.Text = $"You're about to start a match with \n{nickname}, ¿are you ready?";
                lbRetador.Content = $"¡{nickname} has joined the match!";
            }
        }

        private void VerificarEstadoPartida()
        {
            try
            {
                ResponsePartidaDTO response = cliente.ObtenerEstadoPartida(codigoPartida);

                if (response != null && response.success && response.partida != null)
                {
                    var partida = response.partida;
                    bool esAnfitrion = idJugadorActual == partida.IdAdivinador;
                    if (partida.IdEstadoPartida == 7)
                    {
                        if (esAnfitrion)
                        {
                            AjustarTextos(partida.NicknameRetador);
                            /*tbAvisoPartida.Text = $"Estás por comenzar una partida con \n{partida.NicknameRetador}, ¿estás listo?";
                            lbRetador.Content = $"¡{partida.NicknameRetador} se ha unido a la partida!";*/
                        }
                        else
                        {
                            AjustarTextos(partida.Nickname);
                            lbRetador.Visibility = Visibility.Visible;
                            lbAvisoPartida.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (esAnfitrion)
                        {
                            if (IdiomaHelper.IdiomaActual.Equals("es"))
                            {
                                tbAvisoPartida.Text = "Esperando a que un retador se una...";
                            }
                            else
                            {
                                tbAvisoPartida.Text = "Waiting for a challenger...";
                            }
                        }
                        else
                        {
                            tbAvisoPartida.Text = $"Esperando a que {partida.Nickname} comience la partida...";
                        }
                        lbAvisoPartida.Visibility = Visibility.Visible;
                        lbRetador.Visibility = Visibility.Hidden;
                        btnComenzar.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    MessageBox.Show($"Error al verificar el estado: {response?.message ?? "Sin mensaje"}");
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    mainWindow?.CambiarPagina(new ListaPartidasDisponibles());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar el estado: {ex.Message}");
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.CambiarPagina(new ListaPartidasDisponibles());
            }
        }

        private async void CancelarCreacionPartida(object sender, RoutedEventArgs e)
        {
            try
            {
                if (jugadorDTO != null)
                {
                    ResponsePartidaDTO response = await Task.Run(() => cliente.CancelarPartida(codigoPartida));
                    if (response != null && response.success)
                    {
                        string mensaje = $"PARTIDA_DESECHADA:{codigoPartida}";
                        await socketCliente.SendMessageAsync(mensaje);
                        await Task.Delay(500); 
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow?.CambiarPagina(new ListaPartidasDisponibles());
                    }
                    else
                    {
                        MessageBox.Show($"Error al cancelar: {response.message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar: {ex.Message}");
            }
        }

        private async void IniciarPartida(object sender, RoutedEventArgs e)
        {
            try
            {
                isListening = false; 
                socketCliente.MessageReceived -= OnMessageReceived;
                await socketCliente.SendMessageAsync($"INICIAR_PARTIDA:{codigoPartida}");

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.CambiarPagina(new PartidaAhorcado(codigoPartida));
                }
                else
                {
                    MessageBox.Show("Error: MainWindow es null en el anfitrión");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la partida: {ex.Message}");
            }
        }

        private void OnConnectionLost(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.HandleConnectionLost(message ?? "Se ha perdido la conexión con el servidor.");
            });
        }

        private void MostrarAlertaBloqueante(Window alerta)
        {
            alerta.Owner = Window.GetWindow(this);
            alerta.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            alerta.ShowDialog();
        }

    }
}
