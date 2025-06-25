using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class CreacionUsuarioCorrecto : Window
    {
        public CreacionUsuarioCorrecto()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
