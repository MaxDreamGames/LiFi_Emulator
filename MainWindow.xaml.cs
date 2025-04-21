using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace LIFi_Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        bool isPlaying = false;


        BinaryConverter binaryConverter = new BinaryConverter();
        double left, top = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Play(object sender, RoutedEventArgs e)
        {
            if (CodeCanvas.Children.Count == 0) return;
            ClearBitsBackgrounds();
            BitSizeSlider.IsEnabled = false;
            isPlaying = true;
            ProgressLabel.Content = string.Empty;
            ProgressBar.Value = 0;
            int cnt = 0;
            byte bitCnt = 0;
            int size = CodeCanvas.Children.Count;
            foreach (var child in CodeCanvas.Children)
            {
                if (!isPlaying) break;
                if (!(child is BitElement bitElement)) continue;
                int delay = Convert.ToInt32(DelaySlider.Value);
                if (bitCnt == 8)
                {
                    bitCnt = 0;
                    await Task.Delay(delay * 2);
                }
                bitElement.Background = Brushes.Blue;
                Lamp.Fill = bitElement.Minilamp.Fill;
                await Task.Delay(delay);
                Lamp.Fill = Brushes.Transparent;
                await Task.Delay(delay);
                bitCnt++;
                cnt++;
                double perc = (double)((double)cnt / (double)size) * 100;
                ProgressBar.Value = perc;
                ProgressLabel.Content = $"{cnt}/{size}";
            }
            isPlaying = false;
            BitSizeSlider.IsEnabled = true;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            isPlaying = false;
            BitSizeSlider.IsEnabled = true;
            ProgressLabel.Content = string.Empty;
            ProgressBar.Value = 0;
            StatusBar.Visibility = Visibility.Visible;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ProgressLabel.Content = string.Empty;
            ProgressBar.Value = 0;
            StatusBar.Visibility = Visibility.Visible;
            CodeCanvas.Children.Clear();
            top = 0;
            left = 0;
            double bitElementWidth = BitSizeSlider.Value;
            BitSizeSlider.IsEnabled = false;
            if (Input.Text != string.Empty)
            {
                string text = Input.Text;
                string[] decodedText = binaryConverter.TextToBinary(text);
                int size = decodedText.Length;
                int cnt = 0;
                foreach (string decodedValue in decodedText)
                {
                    for (int i = 0; i < decodedValue.Length; i++)
                    {
                        bool isOn = decodedValue[i] == '1';
                        BitElement bitElement = new BitElement();
                        bitElement.Width = bitElementWidth;
                        bitElement.Height = bitElementWidth;
                        bitElement.Minilamp.Width = bitElementWidth * 0.83;
                        bitElement.Minilamp.Height = bitElementWidth * 0.83;
                        bitElement.Minilamp.Fill = isOn ? Brushes.Yellow : Brushes.Gray;
                        if (left > CodeCanvas.ActualWidth - bitElementWidth)
                        {
                            left = 0;
                            top += bitElementWidth;
                        }
                        Canvas.SetLeft(bitElement, left);
                        Canvas.SetTop(bitElement, top);
                        left += bitElementWidth;
                        CodeCanvas.Height = top;
                        CodeCanvas.Children.Add(bitElement);
                        await Task.Delay(10);
                    }
                    cnt++;
                    double perc = (double)((double)cnt / (double)size) * 100;
                    ProgressBar.Value = perc;
                    ProgressLabel.Content = $"{cnt}/{size}";
                    left += bitElementWidth;
                }
            }
            else if (Deb.Content != string.Empty || Deb.Content != null)
            {
                var bytes = binaryConverter.ImageToBinary(Deb.Content.ToString());
                int size = bytes.Count;
                int cnt = 0;
                for (int i = 0; i < bytes.Count; i++)
                {
                    string[] pixel = bytes[i];
                    for (int j = 0; j < pixel.Length; j++)
                    {
                        string color = pixel[j];
                        for (int k = 0; k < color.Length; k++)
                        {
                            bool isOn = color[k] == '1';
                            BitElement bitElement = new BitElement();
                            bitElement.Width = bitElementWidth;
                            bitElement.Height = bitElementWidth;
                            bitElement.Minilamp.Width = bitElementWidth * 0.83;
                            bitElement.Minilamp.Height = bitElementWidth * 0.83;
                            bitElement.Minilamp.Fill = isOn ? Brushes.Yellow : Brushes.Gray;
                            if (left > CodeCanvas.ActualWidth - bitElementWidth)
                            {
                                left = 0;
                                top += bitElementWidth;
                            }
                            Canvas.SetLeft(bitElement, left);
                            Canvas.SetTop(bitElement, top);
                            left += bitElementWidth;
                            CodeCanvas.Height = top;
                            CodeCanvas.Children.Add(bitElement);
                            await Task.Delay(1);
                        }
                        left += bitElementWidth;
                    }
                    cnt++;
                    double perc = (double)((double)cnt / (double)size) * 100;
                    ProgressBar.Value = perc;
                    ProgressLabel.Content = $"{cnt}/{size}";
                    left = 0;
                    top += bitElementWidth;
                    Deb.Content = i;
                }
            }
            BitSizeSlider.IsEnabled = true;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            CodeCanvas.Children.Clear();
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DelayLabel != null)
                DelayLabel.Content = $"{Convert.ToUInt32(e.NewValue)}ms";
        }

        private void BitSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BitSizeLabel != null)
            {
                BitSizeLabel.Content = $"{Convert.ToUInt32(e.NewValue)}px";
                BitSizeSlider.IsEnabled = false;
                ResizeBitElements();
                BitSizeSlider.IsEnabled = true;
            }
        }

        void ClearBitsBackgrounds()
        {
            foreach (BitElement bEl in CodeCanvas.Children)
            {
                bEl.Background = Brushes.Transparent;
            }
        }

        private void ChooseImg(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif", DefaultExt = ".png" };

            var res = ofd.ShowDialog();

            if (res == true)
            {
                Input.Text = string.Empty;
                Deb.Content = ofd.FileName;
            }
        }

        void ResizeBitElements()
        {
            if (CodeCanvas.Children.Count == 0) return;
            List<BitElement> bitElements = new List<BitElement>();
            foreach (var bitElement in CodeCanvas.Children)
            {
                bitElements.Add(bitElement as BitElement);
            }
            CodeCanvas.Children.Clear();
            top = 0;
            left = 0;
            byte bitCnt = 0;
            foreach (BitElement bitElement in bitElements)
            {
                bitElement.Width = BitSizeSlider.Value;
                bitElement.Height = BitSizeSlider.Value;

                bitElement.Minilamp.Width = BitSizeSlider.Value * 0.83;
                bitElement.Minilamp.Height = BitSizeSlider.Value * 0.83;

                if (bitCnt == 8)
                    left += BitSizeSlider.Value;
                if (left > CodeCanvas.ActualWidth - BitSizeSlider.Value)
                {
                    left = 0;
                    if (bitCnt == 8)
                        left = BitSizeSlider.Value;
                    top += BitSizeSlider.Value;
                }
                Canvas.SetLeft(bitElement, left);
                Canvas.SetTop(bitElement, top);
                left += BitSizeSlider.Value;
                CodeCanvas.Height = top;
                CodeCanvas.Children.Add(bitElement);
                if (bitCnt == 8) bitCnt = 0;
                bitCnt++;
            }
        }
    }
}