using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class SinConexionServidor : Window
    {
        public SinConexionServidor()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
