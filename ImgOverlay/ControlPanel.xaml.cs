using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        bool isOpaque;
        public ControlPanel()
        {
            InitializeComponent();
            EnableImageControls();
        }

        private void DragButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner == null)
                return;

            var opaque = (sender as ToggleButton).IsChecked.Value;
            Owner.IsHitTestVisible = opaque;
            isOpaque = opaque;

            var hwnd = new WindowInteropHelper(Owner).Handle;
            if (opaque)
            {
                (Owner as MainWindow)?.ChangeResizeMode(ResizeMode.CanResizeWithGrip);
                WindowsServices.SetWindowExOpaque(hwnd);
            }
            else
            {
                (Owner as MainWindow)?.ChangeResizeMode(ResizeMode.CanResize);
                WindowsServices.SetWindowExTransparent(hwnd);
            }

            e.Handled = true;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                (Owner as MainWindow)?.ChangeResizeMode(isOpaque ? ResizeMode.CanResizeWithGrip : ResizeMode.CanResize);
                (Owner as MainWindow)?.LoadImage(openDialog.FileName);
                EnableImageControls();
            }
        }

        private void EnableImageControls()
        {
            Control[] controls = { DragButton, SizeButton, OpacitySlider, RotateSlider, HideButton, FlipButton };
            bool enable = false;
            if(((Owner as MainWindow)?.ImageIsLoaded).HasValue)
            {
                enable = ((Owner as MainWindow)?.ImageIsLoaded).Value;
            }
            foreach (Control control in controls)
            {
                control.IsEnabled = enable;
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpacityText.Text = Math.Floor(e.NewValue * 100).ToString();
            (Owner as MainWindow)?.ChangeOpacity((float)e.NewValue);
        }

        private void RotateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotateText.Text = e.NewValue.ToString();
            (Owner as MainWindow)?.ChangeRotation((float)e.NewValue);
        }

        private void ControlPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                if (s.Length == 1)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ControlPanel_Drop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s.Length == 1)
            {
                (Owner as MainWindow)?.LoadImage(s[0]);
            }
        }

        private void SizeButton_Click(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.ActualSize();
        }

        private void FlipButton_Click(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.HorizontalFlip();
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            if (HideButton.IsChecked.HasValue)
            {
                (Owner as MainWindow).Show(!HideButton.IsChecked.Value);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.V))
            {
                (Owner as MainWindow).LoadClipboard();
                EnableImageControls();
            }

            //if (DragButton.IsChecked.Value && DragButton.IsEnabled)
            //{
            //    if (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.A)) 
            //    {
            //        (Owner as MainWindow).NudgeX(-1);
            //    }
            //    if (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.D))
            //    {
            //        (Owner as MainWindow).NudgeX(1);
            //    }
            //    if (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.W))
            //    {
            //        (Owner as MainWindow).NudgeY(-1);
            //    }
            //    if (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.S))
            //    {
            //        (Owner as MainWindow).NudgeY(1);
            //    }

            //}


        }

        private void OpacityText_TextChanged(object sender, TextChangedEventArgs e)
        {
            float newValue;
            bool isNumber = float.TryParse(OpacityText.Text, out newValue);
            if (isNumber)
            {
                newValue /= 100.0f;
                //OpacitySlider.Value = newValue;
                (Owner as MainWindow)?.ChangeOpacity(newValue);
            }
        }

        private void RotateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            float newValue;
            bool isNumber = float.TryParse(RotateText.Text, out newValue);
            if (isNumber)
            {
                newValue = Math.Min(Math.Max(newValue, -180), 180);
                (Owner as MainWindow)?.ChangeRotation(newValue);
            }
        }
    }
}
