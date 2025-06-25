using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class CorreoElectronicoErroneo : Window
    {
        public CorreoElectronicoErroneo()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
