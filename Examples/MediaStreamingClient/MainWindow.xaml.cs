using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
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
        private MediaStreamingClientCore.MediaStreamingClient stream;
        public MainWindow()
        {
            InitializeComponent();
            stream = new MediaStreamingClientCore.MediaStreamingClient(true, "localhost", 5300, "ws");
            stream.IgnoreSSL = true;
            //Main
            stream.OnStart += Stream_OnStart;
            stream.OnStop += Stream_OnStop;
        }

        private void ItemButton_Connect_Click(object sender, RoutedEventArgs e)
        {
            stream.Connect();

            //Notification
            stream.Notification.OnStart += Notification_OnStart;
            stream.Notification.OnReceiveData += Notification_OnReceiveData;
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

        #region Stream actions
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

        private void Notification_OnReceiveData(ClientWebSocket socket, byte[] data)
        {
            Dispatcher.Invoke(() =>
            {
                ItemStackPanel_Notification.Children.Add(new Label() { Content = Encoding.UTF8.GetString(data) });
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
            MessageBox.Show("Connecting to 'Notification'");
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
    }
}
