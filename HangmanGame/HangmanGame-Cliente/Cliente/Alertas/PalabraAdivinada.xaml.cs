using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class PalabraAdivinada : Window
    {
        public PalabraAdivinada()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
