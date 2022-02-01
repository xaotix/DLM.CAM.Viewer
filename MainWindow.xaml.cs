using DLMHelix;
using FirstFloor.ModernUI.Windows.Controls;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DLMcam;
using System.Diagnostics;
using System.IO;

namespace VisualizadorCAM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow(string Pasta, SearchOption searchOption)
        {
            InitializeComponent();
            Inicializar();

            this.tab_criar.Visibility = Visibility.Collapsed;
            if(Directory.Exists(Pasta))
            {
            this.SetPasta(Pasta, searchOption);
            }
        }

        private void Inicializar()
        {
            this.Title = System.Windows.Forms.Application.ProductName + " v." + System.Windows.Forms.Application.ProductVersion;
            this.Tipo.ItemsSource = Enum.GetValues(typeof(DLMenum.CAM_PERFIL_TIPO)).Cast<DLMenum.CAM_PERFIL_TIPO>().OrderBy(x => x.ToString()).ToList().FindAll(x => !x.ToString().StartsWith("_"));
            if (this.Tipo.Items.Count > 0)
            {
                this.Tipo.SelectedIndex = 0;
            }
            this.DataContext = this;
        }

        public MainWindow()
        {
            InitializeComponent();
            Inicializar();
            this.DataContext = this;
        }
        public double Comprimento { get; set; } = 5000;
        public double Largura { get; set; } = 150;
        public double Largura2 { get; set; } = 200;

        public double Espessura { get; set; } = 6.35;
        public double Alma { get; set; } = 4.75;
        public double Espessura2 { get; set; } = 7.90;

        public double Altura { get; set; } = 450;
        public double AbaS { get; set; } = 50;
        public double AbaI { get; set; } = 25;
        private void abre_cam(object sender, RoutedEventArgs e)
        {
            var arq = Conexoes.Utilz.Abrir_String("cam", "Selecione um arquivo", "");
            if (arq == "" | arq == null)
            {
                return;
            }


            this.view.Abrir(arq);


        }



        private void cria_banzo(object sender, RoutedEventArgs e)
        {

        }
        public Cam camrender { get; set; }
        private void gerar_cam(object sender, RoutedEventArgs e)
        {
            getCNC();
            if(camrender!=null)
            {
                camrender.GerareVisualizar();
            }
        }

        private void getCNC()
        {
            var TipoPerfil = (DLMenum.CAM_PERFIL_TIPO)Tipo.SelectedItem;

            var ARQ = Conexoes.Utilz.RaizAppData() + @"\" + TipoPerfil.ToString().ToUpper().Replace("_", "") + ".CAM";

            this.camrender = new Cam(ARQ, DLMcam.Perfil.Criar(this.Comprimento,TipoPerfil,this.Altura,this.Alma,this.Largura,this.Espessura,this.AbaS,this.Largura2,this.AbaI,this.Espessura2));
            var s = this.camrender.Formato.Peso;

        }

        private void ver_pasta(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Conexoes.Utilz.RaizAppData());
            }
            catch (Exception)
            {
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderCAM();
        }

        private void RenderCAM()
        {
            getCNC();
            if (camrender != null)
            {
                try
                {
                    this.view.Abrir(camrender);
                }
                catch (Exception)
                {

                }
            }
        }

        private void Tipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RenderCAM();
        }

        private void carregar_cam(object sender, SelectionChangedEventArgs e)
        {
            var s = this.cams_lista.SelectedItem;
            if(s is ReadCam)
            {
                try
                {
                    this.camrender = null;
                    this.view.Abrir((ReadCam)s);

                }
                catch (Exception)
                {

                }
            }
        }

        private void seleciona_pasta(object sender, RoutedEventArgs e)
        {
            var pasta = Conexoes.Utilz.Selecao.SelecionarPasta();

            SetPasta(pasta, SearchOption.TopDirectoryOnly);

        }

        public void SetPasta(string pasta, SearchOption searchOption)
        {
            if (Directory.Exists(pasta))
            {
                var arqs = Conexoes.Utilz.GetArquivos(pasta, "*.CAM", searchOption);
                this.cams_lista.ItemsSource = null;
                this.cams_lista.ItemsSource = arqs.Select(x => new ReadCam(x));
                this.pasta_sel.Text = pasta;
            }
        }

        private void visualiza_cam_criado(object sender, RoutedEventArgs e)
        {
            RenderCAM();
        }

        private void desmembra(object sender, RoutedEventArgs e)
        {
            var cam = this.view.Cam.GetCam();
            var ss = cam.Desmembrar();
            if(ss.Count>0)
            {
                ss.Insert(0,cam);
                MainWindow mm = new MainWindow();
                mm.cams_lista.ItemsSource = ss.Select(x=> x.GetReadCam());
                mm.Show();
                mm.cams_lista.SelectedItem = mm.cams_lista.Items[0];
                mm.tab_criar.Visibility = Visibility.Collapsed;
                mm.bt_abre_pasta.Visibility = Visibility.Collapsed;
                mm.pasta_sel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
