using DLM.helix;
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
using DLM.cam;
using System.Diagnostics;
using System.IO;
using DLM.vars;
using Conexoes;

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
            this.DataContext = MVC;
            this.tab_criar.Visibility = Visibility.Collapsed;
            if (Directory.Exists(Pasta))
            {
                this.SetPasta(Pasta, searchOption);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MVC;
        }


        public MVC MVC { get; set; } = new MVC();
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
        private void gerar_cam(object sender, RoutedEventArgs e)
        {
            getCNC();
            if(MVC.CAM != null)
            {
                MVC.CAM.Gerar();
                MVC.CAM.Arquivo.Abrir();
            }
        }

        private void getCNC()
        {
            var ARQ = Cfg.Init.Raiz_AppData+ @"\" + MVC.Perfil.Tipo.ToString().ToUpper().Replace("_", "") + ".CAM";
            this.MVC.CAM = new Cam(ARQ, MVC.Perfil, MVC.Comprimento);
            var s = this.MVC.CAM.Formato.Peso;
        }

        private void ver_pasta(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Cfg.Init.Raiz_AppData);
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
            if (MVC.CAM != null)
            {
                try
                {
                    this.view.Abrir(MVC.CAM);
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
            if(s is ReadCAM)
            {
                try
                {
                    var c = (s as ReadCAM);
                    this.MVC.CAM = c.GetCam();
                    this.view.Abrir(c);
                    this.MVC.SomenteLeitura = true;
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
                this.cams_lista.ItemsSource = arqs.Select(x => new ReadCAM(x));
                this.pasta_sel.Text = pasta;
                if(this.cams_lista.Items.Count>0)
                {
                    this.cams_lista.SelectedIndex = 0;
                }
            }
        }

        private void visualiza_cam_criado(object sender, RoutedEventArgs e)
        {
            RenderCAM();
        }

        private void desmembra(object sender, RoutedEventArgs e)
        {
            var cam = this.view.Cam.GetCam();
            var ss = cam.Desmembrar(true);
            if(ss.Count>0)
            {
                //ss.Insert(0,cam);
                MainWindow mm = new MainWindow();
                mm.cams_lista.ItemsSource = ss.Select(x=> x.GetReadCam());
                mm.Show();
                mm.cams_lista.SelectedItem = mm.cams_lista.Items[0];
                mm.tab_criar.Visibility = Visibility.Collapsed;
                mm.bt_abre_pasta.Visibility = Visibility.Collapsed;
                mm.pasta_sel.Visibility = Visibility.Collapsed;
            }
        }

        private void define_pasta(object sender, KeyEventArgs e)
        {
            if(e.Key!= Key.Enter) { return; }
            if(pasta_sel.Text.Existe() && Conexoes.Utilz.E_Diretorio(pasta_sel.Text))
            {
                SetPasta(pasta_sel.Text, SearchOption.TopDirectoryOnly);
            }
        }

        private void editar_perfil(object sender, RoutedEventArgs e)
        {
            MVC.Perfil.Propriedades();
            RenderCAM();
        }

        private void set_comprimento(object sender, RoutedEventArgs e)
        {
            this.MVC.Comprimento = this.MVC.Comprimento.Prompt();
            RenderCAM();
        }
    }
    public class MVC:Notificar
    {
        private Cam _CAM { get; set; }
        public Cam CAM
        {
            get
            {
                return _CAM;
            }
            set
            {
                _CAM = value;
                Perfil = value.Perfil;
                NotifyPropertyChanged();
            }
        }

        private bool _SomenteLeitura { get; set; } = false;
        public bool SomenteLeitura
        {
            get
            {
                return _SomenteLeitura;
            }
            set
            {
                _SomenteLeitura = value;
                NotifyPropertyChanged();
            }
        }
        private double _Comprimento { get; set; } = 5000;
        public double Comprimento
        {
            get
            {
                return _Comprimento;
            }
            set
            {
                _Comprimento = value;
                NotifyPropertyChanged();
            }
        }
        private DLM.cam.Perfil _Perfil { get; set; } = new Perfil()
        {
            Tipo = CAM_PERFIL_TIPO.Caixao,
            Altura = 450,
            Largura_MS = 175,
            Largura_MI = 150,
            Esp = 4.75,
            Esp_MS = 12.7,
            Esp_MI = 6.35,
            Raio = 4.75,
            Caixao_Entre_Almas = 90
        };
        public DLM.cam.Perfil Perfil
        {
            get
            {
                return _Perfil;
            }
            set
            {
                _Perfil = value;
                NotifyPropertyChanged();
            }
        }
        public MVC()
        {

        }
    }
}
