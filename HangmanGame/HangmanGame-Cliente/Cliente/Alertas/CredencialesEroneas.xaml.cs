using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class CredencialesEroneas : Window
    {
        public CredencialesEroneas()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
