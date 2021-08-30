using MediaStreaming.Client.Core.Models;
using MediaStreaming.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaStreaming.Client.Core.Modules
{
    public delegate void AvtionUpdateScreen(Bitmap data);
    public sealed class ScreenSharingModule : Module
    {
        protected override string ModuleName => "screen";
        private Screen selectedScreen;
        private Bitmap lastScreenData;

        private ClientWebSocket ScreenStream { get; set; } = new ClientWebSocket();

        public List<ViewStreamModel> ViewStreams { get; } = new List<ViewStreamModel>();

        public Screen[] ScreenList { get => Screen.AllScreens; }
        public bool IsViewPreview { get; set; } = true;

        public event AvtionUpdateScreen UpdatePreview;

        public ScreenSharingModule(string ConnectWsHost, ref Client client)
            : base(ConnectWsHost, ref client)
        {
            ScreenStream.Options.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => IgnoreSSL;
        }

        public void SetScreen(Screen screen)
        {
            if (!ScreenList.Any(p => p == screen))
                throw new Exception("Screen not find!");
            selectedScreen = screen;
        }

        public override void Start()
        {
            ScreenStream.ConnectAsync(new Uri($"{ConnectWsRootUrl}/screen/stream?id={Client.Id}&token={Token}&room={Client.Room}"), CancellationToken.None).Wait();
            _start();
            StartStream();
        }

        public ViewStreamModel ViewSream(string id)
        {
            var viewStream = new ViewStreamModel(id, Client, ConnectWsRootUrl, Token);
            viewStream.IgnoreSSL = IgnoreSSL;
            ViewStreams.Add(viewStream);
            return viewStream;
        }

        private Task StartStream()
        {
            return Task.Factory.StartNew(() =>
            {
                var screen = GetScreen();
                //ScreenStream.SendAsync(imageToByteArray(screen), WebSocketMessageType.Binary, true, CancellationToken.None);
                
                lastScreenData = screen;
                while (Status)
                {
                    screen = GetScreen();

                    if (IsViewPreview)
                        UpdatePreview?.Invoke(screen);
                    //sendFrame(screen);
                    using (var stream = new MemoryStream())
                    {
                        screen.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                        var arr = stream.ToArray();
                        ScreenStream.SendAsync(arr, WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                    }
                    lastScreenData = screen;
                    Task.Delay(34);
                }
            });
        }

        private async void sendFrame(Bitmap bitmap)
        {
            //ScreenStream.SendAsync(arr, WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
            await Task.Run(async () =>
            {
                var newImg = imageToByteArray(bitmap);
                var oldImg = imageToByteArray(lastScreenData);
                for(int i = 0; i < newImg.Length; i++)
                {
                    if(newImg[i] != oldImg[i])
                    {
                        byte[] data = new byte[sizeof(int) + sizeof(byte)];
                        BitConverter.GetBytes(i).CopyTo(data, 0);
                        data[sizeof(int)] = newImg[i];
                        await ScreenStream.SendAsync(data, WebSocketMessageType.Binary, false, CancellationToken.None);
                    }
                }
                await ScreenStream.SendAsync(BitConverter.GetBytes(-1), WebSocketMessageType.Binary, true, CancellationToken.None);
            });
        }

        private Bitmap GetScreen()
        {
            var bitmap = new Bitmap(selectedScreen.WorkingArea.Width, selectedScreen.WorkingArea.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(selectedScreen.WorkingArea.Width, selectedScreen.WorkingArea.Height));
            }
            return bitmap;
        }

        private static byte[] imageToByteArray(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
