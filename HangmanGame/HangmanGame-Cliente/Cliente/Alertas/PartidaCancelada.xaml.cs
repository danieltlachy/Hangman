using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class PartidaCancelada : Window
    {
        public PartidaCancelada()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
