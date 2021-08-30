using MediaStreaming.Client.Core.Models;
using MediaStreaming.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MediaStreamingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaStreaming.Client.Core.MediaStreamingClient stream;
        private Window previewWindow = new Window();
        private System.Windows.Controls.Image previewImage = new System.Windows.Controls.Image();

        public MainWindow()
        {
            InitializeComponent();
            //44326
            //5300
            ItemTextBox_Url.Text = "localhost";
            ItemTextBox_Port.Text = "5300";
            ItemTextBox_RootPath.Text = "ws";
        }

        private void ItemButton_Connect_Click(object sender, RoutedEventArgs e)
        {
            stream = new MediaStreaming.Client.Core.MediaStreamingClient(true, ItemTextBox_Url.Text, uint.Parse(ItemTextBox_Port.Text), ItemTextBox_RootPath.Text, ItemTextBox_Token.Text);
            stream.IgnoreSSL = true;
            //Main
            stream.OnStart += Stream_OnStart;
            stream.OnStop += Stream_OnStop;
            try
            {
                stream.Connect();

                //Notification
                stream.Notification.OnStart += Notification_OnStart;
                stream.Notification.OnReceiveData += Notification_OnReceiveData;
                stream.Notification.OnStartScreenStream += Notification_OnStartScreenStream;
                stream.Notification.Start();
                stream.SetRoom("main");
                //Voice
                stream.Voice.OnStart += Voice_OnStart;
                stream.Voice.OnStop += Voice_OnStop;
                //Video
                stream.Video.OnStart += Video_OnStart;
                stream.Video.OnStop += Video_OnStop;
                //Screen sharing
                stream.ScreenSharing.OnStart += ScreenSharing_OnStart;
                stream.ScreenSharing.OnStop += ScreenSharing_OnStop;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Stream actions
        private void Notification_OnStartScreenStream(NotificationModel data)
        {
            Dispatcher.Invoke(() =>
            {
                var window = new Window();
                ViewStreamModel viewSream = stream.ScreenSharing.ViewSream(data.JsonData["id"].ToString());
                var img = new System.Windows.Controls.Image();
                img.Width = 800;
                img.Height = 600;
                viewSream.OnReceiveData += (ClientWebSocket socket, byte[] buffer) =>
                {
                    previewWindow.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            BitmapImage bitmapimage = new BitmapImage();
                            using (MemoryStream memory = new MemoryStream(buffer, 0, buffer.Length))
                            {
                                memory.Position = 0;
                                bitmapimage.BeginInit();
                                bitmapimage.StreamSource = memory;
                                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapimage.EndInit();
                            }

                            img.Source = bitmapimage;
                        }
                        catch { }
                    });
                };

                viewSream.OnStart += () =>
                {
                    window.Dispatcher.Invoke(() =>
                    {
                        window.Content = img;
                    });
                };

                viewSream.OnStop += () =>
                {
                    window.Dispatcher.Invoke(() =>
                    {
                        window.Close();
                    });
                };

                window.Title = data.ClientId;
                window.Show();

                viewSream.Start();
            });
        }
        private void Video_OnStop()
        {
            Dispatcher.Invoke(() =>
            setStatus(false, ref ItemButton_Video_Start, ref ItemButton_Video_Stop, ref ItemLable_Video_Status));
        }

        private void Video_OnStart()
        {
            Dispatcher.Invoke(() =>
            setStatus(true, ref ItemButton_Video_Start, ref ItemButton_Video_Stop, ref ItemLable_Video_Status));
        }

        private void Voice_OnStop()
        {
            Dispatcher.Invoke(() =>
            setStatus(false, ref ItemButton_Voice_Start, ref ItemButton_Voice_Stop, ref ItemLable_Voice_Status));
        }

        private void Voice_OnStart()
        {
            Dispatcher.Invoke(() =>
            setStatus(true, ref ItemButton_Voice_Start, ref ItemButton_Voice_Stop, ref ItemLable_Voice_Status));
        }

        private void Notification_OnReceiveData(ClientWebSocket socket, byte[] buffer)
        {
            Dispatcher.Invoke(() =>
            {
                var str = Encoding.UTF8.GetString(buffer);
                ItemStackPanel_Notification.Children.Add(new Label() { Content = str });
            });
        }

        private void Stream_OnStop()
        {
            Dispatcher.Invoke(() =>
                setStatus(false, ref ItemButton_Connect, ref ItemButton_Disconnect, ref ItemLable_Status));
        }

        private void Stream_OnStart()
        {
            Dispatcher.Invoke(() =>
                setStatus(true, ref ItemButton_Connect, ref ItemButton_Disconnect, ref ItemLable_Status));
        }

        private void ScreenSharing_OnStop()
        {
            Dispatcher.Invoke(() =>
                setStatus(false, ref ItemButton_ScreenSharing_Start, ref ItemButton_ScreenSharing_Stop, ref ItemLable_ScreenSharing_Status));
        }

        private void ScreenSharing_OnStart()
        {
            Dispatcher.Invoke(() =>
               setStatus(true, ref ItemButton_ScreenSharing_Start, ref ItemButton_ScreenSharing_Stop, ref ItemLable_ScreenSharing_Status));
        }

        private void Notification_OnStart()
        {
            Dispatcher.Invoke(() => 
                ItemStackPanel_Notification.Children.Add(new Label() { Content = "Done connect!" }));
        }
        #endregion

        private void ItemButton_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            stream.Disconnect();
        }

        private void ItemButton_Voice_Start_Click(object sender, RoutedEventArgs e)
        {
            stream.Voice.Start();
        }

        private void ItemButton_Voice_Stop_Click(object sender, RoutedEventArgs e)
        {
            stream.Voice.Stop();
        }

        private void setStatus(bool isStart, ref Button start, ref Button stop, ref Label status)
        {
            start.IsEnabled = !isStart;
            stop.IsEnabled = isStart;
            status.Content = isStart ? "Done connect!" : "Disconnect!";
        }

        private void ItemSlider_SetSensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ItemTextBox_Sensitivity.Text = Math.Round(e.NewValue, 8).ToString();
        }

        private void ItemTextBox_Sensitivity_TextChanged(object sender, TextChangedEventArgs e)
        {
            double number = 0;
            if (double.TryParse((e.OriginalSource as TextBox).Text.ToString(), out number))
            {
                if(ItemSlider_SetSensitivity != null)
                    ItemSlider_SetSensitivity.Value = number;
                if (stream != null && stream.Voice != null)
                    stream.Voice.Sensitivity = Math.Round(number, 8);
            }
        }

        private void ItemButton_ScreenSharing_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                previewWindow.Content = previewImage;
                previewWindow.Title = "Me";
                previewWindow.Show();
                stream.ScreenSharing.UpdatePreview += ScreenSharing_UpdatePreview;
                stream.ScreenSharing.SetScreen(stream.ScreenSharing.ScreenList.First());
                stream.ScreenSharing.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ItemButton_ScreenSharing_Start_Click", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScreenSharing_UpdatePreview(Bitmap data)
        {
            if (Dispatcher == null)
                return;
            Dispatcher.Invoke(() =>
            {
                BitmapImage bitmapimage = new BitmapImage();
                using (MemoryStream memory = new MemoryStream())
                {
                    data.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;             
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                }
                previewImage.Source = bitmapimage;
                previewImage.Width = 800;
                previewImage.Height = 600;
            });
        }

        private void ItemButton_ScreenSharing_Stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItemTextBox_Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
