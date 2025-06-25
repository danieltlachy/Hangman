using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class CamposVacios : Window
    {
        public CamposVacios()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
