using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HangmanGame_Cliente.Cliente.Alertas
{
    public partial class CorreoElectronicoEnUso : Window
    {
        public CorreoElectronicoEnUso()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
