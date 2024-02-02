using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageCopyWeb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BitmapSource> imagensArmazenadas = new List<BitmapSource>();
        private int indiceAtual = -1;  // Índice da imagem atual na lista
        private DispatcherTimer clipboardMonitorTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Configuração do DispatcherTimer para monitorar a área de transferência
            clipboardMonitorTimer = new DispatcherTimer();
            clipboardMonitorTimer.Tick += DetectarEArmazenarImagemNaAreaDeTransferencia;
            clipboardMonitorTimer.Interval = TimeSpan.FromMilliseconds(500);
            clipboardMonitorTimer.Start();






        }

        private void NavegarImagem(int direcao)
        {
            if (imagensArmazenadas.Count > 0)
            {
                // Atualizar o índice da imagem atual
                indiceAtual = (indiceAtual + direcao + imagensArmazenadas.Count) % imagensArmazenadas.Count;

                // Exibir a imagem atual
                ExibirImagem(imagensArmazenadas[indiceAtual]);
            }
        }

        private void DetectarEArmazenarImagemNaAreaDeTransferencia(object sender, EventArgs e)
        {
            try
            {

                if (Clipboard.ContainsImage())
                {

                    BitmapSource novaImagem = Clipboard.GetImage();


                    if (!ImagemJaArmazenada(novaImagem))
                    {
                        imagensArmazenadas.Add(novaImagem);


                        if (imagensArmazenadas.Count > 10)
                        {
                            imagensArmazenadas.RemoveAt(0);

                            indiceAtual--;
                        }


                        indiceAtual = imagensArmazenadas.Count - 1;


                        ExibirImagem(novaImagem);


                        Clipboard.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao verificar a área de transferência: " + ex.Message, "Clipboard Image Example");
            }
        }

        private bool ImagemJaArmazenada(BitmapSource novaImagem)
        {
            foreach (BitmapSource imagemArmazenada in imagensArmazenadas)
            {
                if (SaoImagensIguais(novaImagem, imagemArmazenada))
                {
                    return true;
                }
            }
            return false;
        }

        private bool SaoImagensIguais(BitmapSource imagem1, BitmapSource imagem2)
        {

            return object.ReferenceEquals(imagem1, imagem2);
        }

        private void ExibirImagem(BitmapSource bitmapSource)
        {

            imageControl.Source = bitmapSource;
            positionTextBlock.Text = $"{indiceAtual + 1} de {imagensArmazenadas.Count}";
        }

        private void chkDesabilitarMonitoramento(object sender, RoutedEventArgs e)
        {
            clipboardMonitorTimer.Stop();
        }

        private void chkHabilitarMonitoramento(object sender, RoutedEventArgs e)
        {
            clipboardMonitorTimer.Start();
        }

        private void btnEsquerda(object sender, RoutedEventArgs e)
        {
            NavegarImagem(-1);
        }

        private void btnDireita(object sender, RoutedEventArgs e)
        {
            NavegarImagem(1);
        }



        private void Dowload(object sender, RoutedEventArgs e)
        {
            if (imagensArmazenadas.Count > 0)
            {
                try
                {
                    for (int i = 0; i < imagensArmazenadas.Count; i++)
                    {
                        BitmapSource imagemParaSalvar = imagensArmazenadas[i];

                        // Permite que o usuário escolha o nome do arquivo
                        SaveFileDialog saveFileDialogPerImage = new SaveFileDialog();
                        saveFileDialogPerImage.Filter = "Arquivos de Imagem|.jpg;.jpeg|Todos os arquivos|.";
                        saveFileDialogPerImage.Title = $"Salvar Imagem {i + 1}";
                        saveFileDialogPerImage.FileName = $"imagem_{i + 1}.jpg"; // Nome padrão do arquivo

                        bool? perImageResult = saveFileDialogPerImage.ShowDialog();

                        if (perImageResult == true)
                        {
                            // Combina o caminho do diretório e o nome do arquivo escolhido
                            string filePath = saveFileDialogPerImage.FileName;

                            // Cria o FileStream com o caminho do arquivo
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                // Utiliza o codificador JPEG com qualidade ajustável (0.7 é um valor de exemplo)
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(imagemParaSalvar));
                                encoder.QualityLevel = 40; // Ajuste a qualidade conforme necessário
                                encoder.Save(fileStream);
                            }

                            MessageBox.Show($"Imagem {i + 1} salva com sucesso em:\n{filePath}", "Download");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao salvar as imagens: " + ex.Message, "Clipboard Image Example");
                }
            }
            else
            {
                MessageBox.Show("Não há imagens para salvar.", "Download");
            }
        }


        private void btnExcluirImagem(object sender, RoutedEventArgs e)
        {
            if (imagensArmazenadas.Count > 0 && indiceAtual >= 0 && indiceAtual < imagensArmazenadas.Count)
            {
                // Remove a imagem da lista
                imagensArmazenadas.RemoveAt(indiceAtual);

                // Verifica se o índice atual está dentro dos limites após a exclusão
                indiceAtual = Math.Min(indiceAtual, imagensArmazenadas.Count - 1);

                // Exibe a imagem após a exclusão ou limpa o controle se não houver mais imagens
                if (imagensArmazenadas.Count > 0)
                {
                    ExibirImagem(imagensArmazenadas[indiceAtual]);
                }
                else
                {
                    imageControl.Source = null;
                    positionTextBlock.Text = "0 de 0";
                }
            }
        }






    }
}
