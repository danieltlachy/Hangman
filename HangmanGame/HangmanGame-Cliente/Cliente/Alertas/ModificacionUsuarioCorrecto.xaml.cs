using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{

    public partial class ModificacionUsuarioCorrecto : Window
    {
        public ModificacionUsuarioCorrecto()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
