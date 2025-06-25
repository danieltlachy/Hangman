using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class ContraseniaErronea : Window
    {
        public ContraseniaErronea()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
