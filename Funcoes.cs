using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualizadorCAM
{
    public static class Funcoes
    {
        public static void AbrirPasta(string pasta, SearchOption searchOption)
        {
            MainWindow pp = new MainWindow(pasta, searchOption);
            pp.Show();
        }
    }
}
