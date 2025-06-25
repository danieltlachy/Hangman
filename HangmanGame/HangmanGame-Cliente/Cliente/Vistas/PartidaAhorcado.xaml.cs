using Biblioteca.DTO;
using HangmanGame_Cliente.Cliente.Alertas;
using HangmanGame_Cliente.HangmanServicioReferencia;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HangmanGame_Cliente.Cliente.Vistas
{
    public partial class PartidaAhorcado : Page
    {
        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private string codigoPartida;
        private int idJugadorActual;
        private JugadorDTO jugadorDTO;
        private bool isListening;
        private bool isInitialized;
        private string palabra; 
        private char[] palabraMostrada; 
        private int intentosFallidos = 0; 
        private const int maxIntentosFallidos = 6; 
        private Label[] labelsPalabra; 
        private bool esAnfitrion; 
        private int idOtroJugador; 
        private Label[] partesMuneco;
        private int idPartidaActual;
        private int jugadorAbandono;

        public PartidaAhorcado(string codigoPartida)
        {
            InitializeComponent();
            this.codigoPartida = codigoPartida;
            cliente = new HangmanServiceClient();
            socketCliente = new SocketCliente();
            isListening = true;
            
            labelsPalabra = new Label[]
            {
                labelPrimeraLetra, labelSegundaLetra, labelTerceraLetra, labelCuartaLetra,
                labelQuintaLetra, labelSextaLetra, labelSeptimaLetra, labelOctavaLetra,
                labelNovenaLetra, labelDecimaLetra, labelOnceavaLetra, labelDoceavaLetra
            };

            partesMuneco = new Label[]
            {
                FindName("casco") as Label,
                FindName("pechera") as Label,
                FindName("brazoder") as Label,
                FindName("brazoizq") as Label,
                FindName("botaizq") as Label,
                FindName("botader") as Label
            };

            Loaded += PartidaAhorcado_Loaded;
        }

        private async void PartidaAhorcado_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return;
            isInitialized = true;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                MessageBox.Show("Error: No se encontró MainWindow en PartidaAhorcado."); 
                return;
            }
            jugadorDTO = mainWindow.GetJugadorAutenticado();
            if (jugadorDTO == null)
            {
                MessageBox.Show("Error: No se encontró el jugador autenticado.");
                mainWindow.CambiarPagina(new IniciarSesion());
                return;
            }
            idJugadorActual = jugadorDTO.id_jugador;

            ResponsePartidaDTO response = cliente.ObtenerEstadoPartida(codigoPartida);
            if (response != null && response.success && response.partida != null)
            {
                esAnfitrion = idJugadorActual == response.partida.IdAdivinador;
                idOtroJugador = esAnfitrion ? response.partida.IdRetador : response.partida.IdAdivinador;
                idPartidaActual = response.partida.IdPartida;
                if (IdiomaHelper.IdiomaActual.Equals("es"))
                {
                    tbPista.Text = response.partida.Pista;
                }
                else
                {
                    tbPista.Text = response.partida.PistaEn;
                }

                palabra = response.partida.Palabra.ToUpper();
                int longitudSinEspacios = palabra.Count(c => !char.IsWhiteSpace(c));
                palabraMostrada = new char[longitudSinEspacios];
                for (int i = 0; i < longitudSinEspacios; i++) 
                {
                    palabraMostrada[i] = '_'; 
                }

                AjustarVisibilidadLabels();
                ActualizarPalabraMostrada();

                if (esAnfitrion)
                {
                    DeshabilitarBotonesLetras();
                }
            }
            else
            {
                MessageBox.Show($"Error al obtener el estado de la partida: {response?.message}");
                mainWindow.CambiarPagina(new ListaPartidasDisponibles());
                return;
            }

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
                    mainWindow.CambiarPagina(new ListaPartidasDisponibles());
                });
            }
        }

        private void DeshabilitarBotonesLetras()
        {
            foreach (var child in (this.Content as Grid).Children)
            {
                if (child is Button button && button.Name.StartsWith("btn"))
                {
                    button.IsEnabled = false;
                }
            }
        }

        private void AjustarVisibilidadLabels()
        {
            if (labelsPalabra == null || palabra == null)
            {
                return;
            }

            int longitudPalabra = palabra.Length;
            for (int i = 0; i < labelsPalabra.Length; i++)
            {
                if (i < longitudPalabra)
                {
                    labelsPalabra[i].Visibility = Visibility.Visible; 
                }
                else
                {
                    labelsPalabra[i].Visibility = Visibility.Collapsed; 
                }
            }
        }

        private void ActualizarPalabraMostrada()
        {
            int indicePalabraMostrada = 0;

            for (int i = 0; i < palabra.Length; i++)
            {
                if (char.IsWhiteSpace(palabra[i]))
                {
                    labelsPalabra[i].Content = " "; 
                }
                else
                {
                    if (indicePalabraMostrada < palabraMostrada.Length)
                    {
                        labelsPalabra[i].Content = palabraMostrada[indicePalabraMostrada].ToString();
                        indicePalabraMostrada++;
                    }
                    else
                    {
                        labelsPalabra[i].Content = "_"; 
                    }
                }
            }
        }

        private async void LetterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!esAnfitrion) 
            {
                string letra = button.Content.ToString();
                try
                {
                    button.IsEnabled = false;
                    bool acierto = false;

                    int indiceSinEspacios = 0;
                    for (int i = 0; i < palabra.Length; i++)
                    {
                        if (!char.IsWhiteSpace(palabra[i]))
                        {
                            if (char.ToUpper(palabra[i]) == char.ToUpper(letra[0]))
                            {
                                palabraMostrada[indiceSinEspacios] = char.ToUpper(palabra[i]);
                                acierto = true;
                            }
                            indiceSinEspacios++;
                        }
                    }

                    if (!acierto)
                    {
                        intentosFallidos++;
                        int indiceMuneco = intentosFallidos - 1; 
                        if (indiceMuneco >= 0 && indiceMuneco < maxIntentosFallidos)
                        {
                            MostrarParteMuneco(indiceMuneco);
                        }
                        else
                        {
                            MessageBox.Show($"Índice inválido calculado: {indiceMuneco} para intentosFallidos = {intentosFallidos}");
                        }

                        if (intentosFallidos >= maxIntentosFallidos)
                        {
                            MostrarMunecoCompleto(); 
                            await EnviarFinPartida("RETADOR_PERDIO");
                            return;
                        }
                    }

                    ActualizarPalabraMostrada();

                    string palabraSinEspacios = new string(palabra.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    string palabraMostradaStr = new string(palabraMostrada);
                    

                    // Enviar el estado actualizado a través del socket
                    string palabraMostradaStrSinEspacios = new string(palabraMostrada);
                    string mensaje = $"ACTUALIZAR_PARTIDA:{codigoPartida}:{letra}:{palabraMostradaStrSinEspacios}:{intentosFallidos}";
                    await socketCliente.SendMessageAsync(mensaje);

                    if (palabraMostradaStr == palabraSinEspacios)
                    {
                        await EnviarFinPartida("RETADOR_GANO");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar el clic en {letra}: {ex.Message}");  
                    await EnviarFinPartida("ERROR_JUEGO");
                    TerminarPartida();
                }
            }
        }

        private void MostrarParteMuneco(int indice)
        {
            if (indice >= 0 && indice < partesMuneco.Length)
            {
                partesMuneco[indice].Visibility = Visibility.Visible;
            }
        }

        private void MostrarMunecoCompleto()
        {
            for (int i = 0; i < partesMuneco.Length; i++)
            {
                partesMuneco[i].Visibility = Visibility.Visible;
            }
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (message.StartsWith("ACTUALIZAR_PARTIDA:"))
                    {
                        var partes = message.Split(':');
                        if (partes.Length >= 5 && partes[1] == codigoPartida)
                        {
                            string letra = partes[2];
                            string palabraMostradaRecibida = partes[3];
                            intentosFallidos = int.Parse(partes[4]);

                            int indiceSinEspacios = 0;
                            for (int i = 0; i < palabra.Length && indiceSinEspacios < palabraMostrada.Length; i++)
                            {
                                if (!char.IsWhiteSpace(palabra[i]))
                                {
                                    if (indiceSinEspacios < palabraMostradaRecibida.Length)
                                    {
                                        palabraMostrada[indiceSinEspacios] = palabraMostradaRecibida[indiceSinEspacios];
                                    }
                                    indiceSinEspacios++;
                                }
                            }
                            
                            ActualizarPalabraMostrada();
                            int indiceMuneco = intentosFallidos - 1;
                            if (indiceMuneco >= 0 && indiceMuneco < maxIntentosFallidos)
                            {
                                MostrarParteMuneco(indiceMuneco);
                            }

                            foreach (var child in (this.Content as Grid).Children)
                            {
                                if (child is Button button && button.Name == $"btn{letra}")
                                {
                                    button.IsEnabled = false;
                                    break;
                                }
                            }
                        }
                    }
                    else if (message.StartsWith("FIN_PARTIDA:"))
                    {
                        var partes = message.Split(':');
                        if (partes.Length >= 3 && partes[1] == codigoPartida)
                        {
                            string resultado = partes[2];
                            string mensajeGanador = resultado == "RETADOR_GANO"
                                ? (esAnfitrion ? "El retador ganó la partida." : "¡Ganaste la partida!")
                                : (esAnfitrion ? "Ganaste, el retador perdió." : "Perdiste la partida.");
                            if (resultado == "RETADOR_PERDIO")
                            {
                                MostrarMunecoCompleto();
                                if (esAnfitrion)
                                {
                                    var resultadoResponse = cliente.GuardarResultado(idJugadorActual, idPartidaActual, true, false);
                                    MostrarAlertaBloqueante(new PalabraAdivinada());
                                    TerminarPartida();
                                }
                                else
                                {
                                    var resultadoResponse = cliente.GuardarResultado(idJugadorActual, idPartidaActual, false, false);
                                    MostrarAlertaBloqueante(new PalabraNoAdivinada());
                                    TerminarPartida();
                                }
                            }
                            if(resultado == "RETADOR_GANO")
                            {
                                if (esAnfitrion)
                                {
                                    var resultadoResponse = cliente.GuardarResultado(idJugadorActual, idPartidaActual, false, false);
                                    MostrarAlertaBloqueante(new PalabraNoAdivinada());
                                    TerminarPartida();
                                }
                                else
                                {
                                    var resultadoResponse = cliente.GuardarResultado(idJugadorActual, idPartidaActual, true, false);
                                    MostrarAlertaBloqueante(new PalabraAdivinada());
                                    TerminarPartida();
                                }
                            }
                            if (resultado == "JUGADOR_ABANDONO")
                            {
                                if(jugadorAbandono == idJugadorActual)
                                {
                                    var resultadoResponse = cliente.GuardarResultado(jugadorAbandono, idPartidaActual, false, true);
                                    MostrarMunecoCompleto();
                                    MostrarAlertaBloqueante(new PartidaCancelada());
                                    TerminarPartida();
                                }
                                else
                                {
                                    var resultadoResponse = cliente.GuardarResultado(idJugadorActual, idPartidaActual, false, false);
                                    MostrarMunecoCompleto();
                                    MostrarAlertaBloqueante(new PartidaCancelada());
                                    TerminarPartida();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar mensaje para {(esAnfitrion ? "anfitrión" : "retador")} ID {idJugadorActual}: {ex.Message}");
                    TerminarPartida();
                }
            });
        }

        private async Task EnviarFinPartida(string resultado)
        {
            string mensaje = $"FIN_PARTIDA:{codigoPartida}:{resultado}";
            try
            {
                await socketCliente.SendMessageAsync(mensaje);
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar FIN_PARTIDA: {ex.Message}");
            }
        }

        private void TerminarPartida()
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.CambiarPagina(new ListaPartidasDisponibles());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al terminar la partida: {ex.Message}");
            }
            finally
            {
                if (socketCliente != null && socketCliente.IsConnected)
                {
                    socketCliente.Desconectar();
                    try { cliente.Close(); } catch { cliente.Abort(); }
                }
            }
        }

        private async void AbandonarPartida(object sender, RoutedEventArgs e)
        {
            try
            {
                jugadorAbandono = jugadorDTO.id_jugador;
                string mensaje = $"FIN_PARTIDA:{codigoPartida}:JUGADOR_ABANDONO";
                await socketCliente.SendMessageAsync(mensaje);
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abandonar la partida: {ex.Message}");
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
