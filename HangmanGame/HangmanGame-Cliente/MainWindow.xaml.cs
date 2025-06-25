using Biblioteca.DTO;
using HangmanGame_Cliente.Cliente.Alertas;
using HangmanGame_Cliente.Cliente.Vistas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HangmanGame_Cliente
{
    public partial class MainWindow : Window
    {
        private Page paginaActual;
        private Frame marcoPaginaActual; // Añadir una referencia al Frame
        public JugadorDTO JugadorAutenticado { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            marcoPaginaActual = MarcoPaginaActual; // Asignar el Frame desde el XAML
            paginaActual = new IniciarSesion();
            marcoPaginaActual.Navigate(paginaActual);
        }

        public void CambiarPagina(Page nuevaPagina)
        {
            paginaActual = nuevaPagina;
            marcoPaginaActual.Navigate(nuevaPagina);
        }

        public void SetJugadorAutenticado(JugadorDTO jugadorDTO)
        {
            JugadorAutenticado = jugadorDTO;
        }

        public JugadorDTO GetJugadorAutenticado()
        {
            return JugadorAutenticado;
        }

        public void HandleConnectionLost(string message)
        {
            Dispatcher.Invoke(() =>
            {
                CambiarPagina(new IniciarSesion());
                MostrarAlertaBloqueante(new SinConexionServidor());
            });
        }

        public async Task<T> CallWcfServiceAsync<T>(Func<Task<T>> action, string errorMessage)
        {
            try
            {
                Console.WriteLine("Intentando llamar al servicio WCF...");
                return await action();
            }
            catch (EndpointNotFoundException ex)
            {
                Console.WriteLine($"EndpointNotFoundException: {ex.Message}");
                HandleConnectionLost($"El servidor WCF no está disponible: {ex.Message}");
                return default;
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine($"CommunicationException: {ex.Message}");
                HandleConnectionLost($"Error de comunicación con el servidor WCF: {ex.Message}");
                return default;
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"TimeoutException: {ex.Message}");
                HandleConnectionLost($"Tiempo de espera agotado con el servidor WCF: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción inesperada: {ex.Message}");
                HandleConnectionLost($"Error inesperado con el servidor WCF: {ex.Message}");
                return default;
            }
        }
        private void MostrarAlertaBloqueante(Window alerta)
        {
            alerta.Owner = Window.GetWindow(this);
            alerta.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            alerta.ShowDialog();
        }
    }
}
