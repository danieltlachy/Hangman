using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class CamposErroneos : Window
    {
        public CamposErroneos()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
