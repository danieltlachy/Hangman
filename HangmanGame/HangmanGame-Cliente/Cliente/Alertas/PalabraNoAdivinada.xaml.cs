using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class PalabraNoAdivinada : Window
    {
        public PalabraNoAdivinada()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
