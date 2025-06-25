using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class InicioSesionExitoso : Window
    {
        public InicioSesionExitoso()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
