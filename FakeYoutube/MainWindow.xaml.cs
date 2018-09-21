using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace FakeYoutube
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        Storyboard playAnim, pauseAnim;
        BitmapImage pauseImg, playImg;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += (o, e) => KeyHook.Stop();
            KeyHook.KeyDown += KeyHook_KeyDown;
            KeyHook.Start();

            playAnim = FindResource("PlayAnim") as Storyboard;
            pauseAnim = FindResource("PauseAnim") as Storyboard;
            playAnim.Completed += PlayAnim_Completed;
            pauseImg = new BitmapImage();
            pauseImg.BeginInit();
            pauseImg.UriSource = new Uri("Pause.png", UriKind.Relative);
            pauseImg.EndInit();
            playImg = new BitmapImage();
            playImg.BeginInit();
            playImg.UriSource = new Uri("Play.png", UriKind.Relative);
            playImg.EndInit();
            HideMe();
        }

        private void PlayAnim_Completed(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void KeyHook_KeyDown(RawKeyEventArgs key)
        {
            if (key.Key == Key.F8)
                Switch();
        }

        void Switch()
        {
            if (Visibility == Visibility.Hidden)
                ShowMe();
            else
                HideMe();
        }

        void ShowMe()
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            this.Topmost = true;
            this.Activate();
            this.grid.Background = CopyScreen();
            image.Source = pauseImg;
            BeginStoryboard(pauseAnim);
            Visibility = Visibility.Visible;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Switch();
        }

        void HideMe()
        {
            this.grid.Background = Brushes.Transparent;
            image.Source = playImg;
            BeginStoryboard(playAnim);
            // GC.Collect();
        }

        static ImageBrush CopyScreen()
        {
            using (var screenBmp = new Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                IntPtr hbitmap = IntPtr.Zero; 
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    try
                    {
                        bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                        hbitmap = screenBmp.GetHbitmap();
                        return new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(
                            hbitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions()));
                    }
                    finally
                    {
                        DeleteObject(hbitmap);
                    }
                }
            }
        }
    }
}
