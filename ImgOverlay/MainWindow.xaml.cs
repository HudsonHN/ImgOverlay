﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using WpfAnimatedGif;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ControlPanel cp = new ControlPanel();

        public bool ImageIsLoaded { get; set; } = false;

        public double? ImageSourceHeight { get; set; } = null;
        public double? ImageSourceWidth { get; set; } = null;

        public int globalScaleX = 1;
        public float globalRotation = 0.0f;

        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            InitializeComponent();
        }

        public void ChangeResizeMode(ResizeMode newResizeMode)
        {
            ResizeMode = newResizeMode;
        }

        public void NudgeY(int pixels)
        {
            this.Top += pixels;

        }
        public void NudgeX(int pixels)
        {
            this.Left += pixels;
        }

        public void LoadImage(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Cannot open folders.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("The selected image file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var img = new BitmapImage();
            try
            {
                img.BeginInit();
                img.UriSource = new Uri(path);
                img.EndInit();
                if (path.EndsWith(".gif"))
                {
                    ImageBehavior.SetAnimatedSource(DisplayImage, img);
                    ImageBehavior.SetRepeatBehavior(DisplayImage, RepeatBehavior.Forever);
                }
                else
                {
                    ImageBehavior.SetAnimatedSource(DisplayImage, null);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading image from storage. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ShowImage(img);
        }

        private void ShowImage(BitmapImage img)
        {
            try
            {
                DisplayImage.Source = img;
                ImageIsLoaded = true;
                ImageSourceHeight = img.Height;
                ImageSourceWidth = img.Width;

            }
            catch (Exception)
            {
                MessageBox.Show("Error showing image. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadClipboard()
        {
            BitmapImage bImg = new BitmapImage();
            try
            {
                BitmapSource bitmapSource = Clipboard.GetImage();

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                bImg.EndInit();

                memoryStream.Close();
                bImg.Freeze();

            }
            catch (Exception)
            {
                MessageBox.Show("Error loading image from clipboard. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ShowImage(bImg);
        }

        public void Show(bool visible)
        {
            if (visible)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Visibility.Hidden;
            }
        }

        public void ChangeOpacity(float opacity)
        {
            DisplayImage.Opacity = opacity;
        }

        public void ChangeRotation(float angle)
        {
            // Create a transform to rotate the button
            RotateTransform myRotateTransform = new RotateTransform();

            // Set the rotation of the transform.
            myRotateTransform.Angle = angle;
            globalRotation = angle;

            ScaleTransform myScaleTransform = new ScaleTransform();
            myScaleTransform.ScaleX = globalScaleX; 
            
            // Create a TransformGroup to contain the transforms
            // and add the transforms to it.
            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);

            DisplayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            // Associate the transforms to the button.
            DisplayImage.RenderTransform = myTransformGroup;
        }

        public void ActualSize()
        {
            if (ImageSourceHeight.HasValue && ImageSourceWidth.HasValue)
            {
                this.Width = ImageSourceWidth.Value;
                this.Height = ImageSourceHeight.Value;

            }
        }

        public void HorizontalFlip()
        {
            DisplayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            
            ScaleTransform myScaleTransform = new ScaleTransform();
            myScaleTransform.ScaleX = -globalScaleX;
            globalScaleX = -globalScaleX;

            RotateTransform myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = globalRotation;

            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);

            DisplayImage.RenderTransform = myTransformGroup;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cp.Owner = this;
            cp.Show();
            cp.Closed += (o, ev) =>
            {
                this.Close();
            };
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

                if (e.KeyboardDevice.IsKeyDown(Key.Left))
                {
                    NudgeX(-1);
                }
                if (e.KeyboardDevice.IsKeyDown(Key.Right))
                {
                    NudgeX(1);
                }
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    NudgeY(-1);
                }
                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    NudgeY(1);
                }



        }

    }
}
