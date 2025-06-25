using Biblioteca.DTO;
using HangmanGame_Cliente.Cliente.Alertas;
using HangmanGame_Cliente.HangmanServicioReferencia;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HangmanGame_Cliente.Cliente.Vistas
{
    public partial class FormularioUsuario : Page
    {
        SolidColorBrush rojo = new SolidColorBrush(Colors.Red);
        SolidColorBrush transparente = new SolidColorBrush(Colors.Transparent);
        private HangmanServiceClient cliente;
        private SocketCliente socketCliente;
        private JugadorDTO jugadorExistente;
        private bool isInitialized;
        private bool esEdicion = false;

        public FormularioUsuario(bool esEdicion)
        {
            InitializeComponent();
            this.esEdicion = esEdicion;
            cliente = new HangmanServiceClient();
            Loaded += FormularioUsuario_Loaded;
            Unloaded += FormularioUsuario_Unloaded;
        }

        private async void FormularioUsuario_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return;
            isInitialized = true;

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null)
            {
                MessageBox.Show("Error: No se encontró MainWindow.");
                return;
            }
            jugadorExistente = mainWindow.GetJugadorAutenticado();

            socketCliente = new SocketCliente();
            socketCliente.ConnectionLost += OnConnectionLost;

            try
            {
                await socketCliente.ConectarAsync("127.0.0.1", 12345);
                await socketCliente.SendMessageAsync("MONITOR_ESTADISTICAS");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar al servidor de sockets: " + ex.Message);
                OnConnectionLost(ex.Message);
            }

            cargarInformacion();
        }

        private void FormularioUsuario_Unloaded(object sender, RoutedEventArgs e)
        {
            socketCliente.Desconectar();
            try { cliente.Close(); } catch { cliente.Abort(); }
        }

        private void btnGuardarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarCampos())
            {
                try
                {
                    ResponseDTO response;

                    if (esEdicion)
                    {
                        var jugadorActual = new JugadorDTO()
                        {
                            id_jugador = jugadorExistente.id_jugador,
                            usuario = txtUsuario.Text,
                            nombre = txtNombreCompleto.Text,
                            fecha_nacimiento = DateTime.ParseExact(txtFechaNacimiento.Text, "dd-MM-yyyy", null),
                            telefono = txtTelefono.Text,
                            correo = txtCorreo.Text,
                            contraseña = psBContrasenia.Password
                        };
                        response = cliente.ActualizarJugador(jugadorActual);

                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow.SetJugadorAutenticado(jugadorActual);
                        //var jugadorAutenticado = mainWindow.GetJugadorAutenticado();

                    }
                    else
                    {
                        var jugadorNuevo = new JugadorDTO()
                        {

                            usuario = txtUsuario.Text,
                            nombre = txtNombreCompleto.Text,
                            fecha_nacimiento = DateTime.ParseExact(txtFechaNacimiento.Text, "dd-MM-yyyy", null),
                            telefono = txtTelefono.Text,
                            correo = txtCorreo.Text,
                            contraseña = psBContrasenia.Password
                        };

                        response = cliente.RegistrarJugador(jugadorNuevo);
                    }

                    if (response != null && response.success)
                    {
                        if (esEdicion)
                        {
                            MostrarAlertaBloqueante(new ModificacionUsuarioCorrecto());
                        }
                        else
                        {
                            MostrarAlertaBloqueante(new CreacionUsuarioCorrecto());
                        }

                        NavigationService?.GoBack();
                    }
                    else
                    {
                        MessageBox.Show($"Error al {(esEdicion ? "actualizar" : "registrar")} usuario: {response?.message ?? "Sin mensaje"}");
                    }
                }
                catch (FormatException ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MostrarAlertaBloqueante(new SinConexionBaseDatos());
                        Console.WriteLine("Error: " + ex.Message);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MostrarAlertaBloqueante(new SinConexionBaseDatos());
                        Console.WriteLine("Error: " + ex.Message);
                    });
                }
            }
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void limpiarBordes()
        {
            txtUsuario.BorderBrush = transparente;
            txtNombreCompleto.BorderBrush = transparente;
            txtFechaNacimiento.BorderBrush = transparente;
            txtTelefono.BorderBrush = transparente;
            txtCorreo.BorderBrush = transparente;
            psBContrasenia.BorderBrush = transparente;
        }

        #region Validaciones
        private bool ValidarCampos()
        {
            limpiarBordes();

            bool usuarioVacio = string.IsNullOrWhiteSpace(txtUsuario.Text);
            bool nombreVacio = string.IsNullOrWhiteSpace(txtNombreCompleto.Text);
            bool fechaVacia = string.IsNullOrWhiteSpace(txtFechaNacimiento.Text);
            bool telefonoVacio = string.IsNullOrWhiteSpace(txtTelefono.Text);
            bool correoVacio = string.IsNullOrWhiteSpace(txtCorreo.Text);
            bool contraseniaVacia = string.IsNullOrWhiteSpace(psBContrasenia.Password);

            List<bool> camposVacios = new List<bool>
            {
                usuarioVacio, nombreVacio, fechaVacia,
                telefonoVacio, correoVacio
            };

            if (!esEdicion || (!contraseniaVacia && !string.IsNullOrEmpty(psBContrasenia.Password)))
            {
                camposVacios.Add(contraseniaVacia);
            }

            if (camposVacios.Contains(true))
            {
                if (usuarioVacio) txtUsuario.BorderBrush = rojo;
                if (nombreVacio) txtNombreCompleto.BorderBrush = rojo;
                if (fechaVacia) txtFechaNacimiento.BorderBrush = rojo;
                if (telefonoVacio) txtTelefono.BorderBrush = rojo;
                if (correoVacio) txtCorreo.BorderBrush = rojo;
                if (contraseniaVacia || !string.IsNullOrEmpty(psBContrasenia.Password)) psBContrasenia.BorderBrush = rojo;

                MostrarAlertaBloqueante(new CamposVacios());
                return false;
            }

            if (!Regex.IsMatch(txtUsuario.Text, @"^[a-zA-Z0-9]+$"))
            {
                txtUsuario.BorderBrush = rojo;
                MostrarAlertaBloqueante(new CamposErroneos());
                return false;
            }

            if (!Regex.IsMatch(txtNombreCompleto.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                txtNombreCompleto.BorderBrush = rojo;
                MostrarAlertaBloqueante(new CamposErroneos());
                return false;
            }

            if (!DateTime.TryParseExact(txtFechaNacimiento.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            {
                txtFechaNacimiento.BorderBrush = rojo;
                MostrarAlertaBloqueante(new FechaNacimientoErroneo());
                return false;
            }

            if (!Regex.IsMatch(txtTelefono.Text, @"^\d{10}$"))
            {
                txtTelefono.BorderBrush = rojo;
                MostrarAlertaBloqueante(new CamposErroneos());
                return false;
            }

            if (!Regex.IsMatch(txtCorreo.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                txtCorreo.BorderBrush = rojo;
                MostrarAlertaBloqueante(new CorreoElectronicoErroneo());
                return false;
            }
            
            if ((!string.IsNullOrEmpty(psBContrasenia.Password)))
            {
                string pattern = "^(?=\\w*\\d)(?=\\w*[A-Z])(?=\\w*[a-z])\\S{8,}$";
                if (!Regex.IsMatch(psBContrasenia.Password, pattern))
                {
                    psBContrasenia.BorderBrush = rojo;
                    MostrarAlertaBloqueante(new ContraseniaErronea());
                    return false;
                }
            }

            return true;
        }
        private void txtUsuarioChanged(object sender, TextChangedEventArgs e) //Valida que el nombre de usuario no exceda los 20 caracteres
        {
            if (txtUsuario.Text.Length > 20)
            {
                txtUsuario.Text = txtUsuario.Text.Substring(0, 20);
                txtUsuario.CaretIndex = txtUsuario.Text.Length;
            }
        }

        private void txtNombreCompletoChanged(object sender, TextChangedEventArgs e) {
            if (txtNombreCompleto.Text.Length > 28)
            {
                txtNombreCompleto.Text = txtNombreCompleto.Text.Substring(0, 28);
                txtNombreCompleto.CaretIndex = txtNombreCompleto.Text.Length;
            }
        }

        private void txtFechaNacimientoChanged(object sender, TextChangedEventArgs e) 
        {
            if (txtFechaNacimiento.Text.Length > 10)
            {
                txtFechaNacimiento.Text = txtFechaNacimiento.Text.Substring(0, 10);
                txtFechaNacimiento.CaretIndex = txtFechaNacimiento.Text.Length;
            }
        }

        private void txtTelefonoChanged(object sender, TextChangedEventArgs e) 
        {
            if (txtTelefono.Text.Length > 10)
            {
                txtTelefono.Text = txtTelefono.Text.Substring(0, 10);
                txtTelefono.CaretIndex = txtTelefono.Text.Length;
            }
        }

        private void txtCorreoChanged(object sender, TextChangedEventArgs e) 
        {
            if (txtCorreo.Text.Length > 28)
            {
                txtCorreo.Text = txtCorreo.Text.Substring(0, 28);
                txtCorreo.CaretIndex = txtCorreo.Text.Length;
            }
        }

        #endregion

        public void cargarInformacion()
        {
            if (jugadorExistente != null)
            {
                txtUsuario.Text = jugadorExistente.usuario;
                txtNombreCompleto.Text = jugadorExistente.nombre;
                txtFechaNacimiento.Text = jugadorExistente.fecha_nacimiento.ToString("dd-MM-yyyy");
                txtTelefono.Text = jugadorExistente.telefono.ToString();
                txtCorreo.Text = jugadorExistente.correo;
                txtCorreo.IsEnabled = false;
                psBContrasenia.Password = jugadorExistente.contraseña;
                psBContrasenia.IsEnabled = false;
            }
        }

        private void MostrarAlertaBloqueante(Window alerta)
        {
            alerta.Owner = Window.GetWindow(this);
            alerta.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            alerta.ShowDialog();
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
