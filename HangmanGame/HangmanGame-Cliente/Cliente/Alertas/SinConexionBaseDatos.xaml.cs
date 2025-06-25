using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class SinConexionBaseDatos : Window
    {
        public SinConexionBaseDatos()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
