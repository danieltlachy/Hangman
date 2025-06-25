using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class FechaNacimientoErroneo : Window
    {
        public FechaNacimientoErroneo()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
