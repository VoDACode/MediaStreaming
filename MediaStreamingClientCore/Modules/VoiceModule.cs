using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MediaStreamingClientCore.Modules
{
    public sealed class VoiceModule : Module
    {
        private DebugData debug;
        protected override string ModuleName => "voice";
        private WaveInEvent input;
        private WaveOutEvent output;
        private BufferedWaveProvider bufferStream;

        public VoiceModule(string ConnectWsHost, ref Client client)
            : base(ConnectWsHost, ref client)
        {
            debug = new DebugData("127.0.0.1", 9090);
        }

        public override void Start()
        {
            // debug.Connect();

            //создаем поток для записи нашей речи
            input = new WaveInEvent();
            //определяем его формат - частота дискретизации 22000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            input.WaveFormat = new WaveFormat(22000, 16, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
            input.DataAvailable += Voice_Input;
            //создаем поток для прослушивания входящего звука
            output = new WaveOutEvent();
            //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
            bufferStream = new BufferedWaveProvider(new WaveFormat(22000, 16, 1));
            //привязываем поток входящего звука к буферному потоку
            output.Init(bufferStream);
            //сокет для отправки звука

            OnReceiveData += VoiceModule_OnReceiveData;
            base.Start();
            output.Play();

            input.StartRecording();
        }

        public override void Stop()
        {
            if (output != null)
            {
                output.Stop();
                output.Dispose();
                output = null;
            }
            if (input != null)
            {
                input.Dispose();
                input = null;
            }
            bufferStream = null;
            OnReceiveData -= VoiceModule_OnReceiveData;
            base.Stop();
        }

        private void VoiceModule_OnReceiveData(ClientWebSocket socket, byte[] data)
        {
            try
            {
                
                bufferStream.AddSamples(data, 0, data.Length);
            }
            catch
            { }
        }

        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                if (ProcessData(e, 0.02))
                {
                    debug.Send(e.Buffer);
                    Socket.SendAsync(e.Buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private bool ProcessData(WaveInEventArgs e, double porog = 0.02)
        {
            bool result = false;
            bool Tr = false;
            double Sum2 = 0;
            int Count = e.BytesRecorded / 2;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                double Tmp = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                Tmp /= 32768.0;
                Sum2 += Tmp * Tmp;
                if (Tmp > porog)
                    Tr = true;
            }
            Sum2 /= Count;
            if (Tr || Sum2 > porog)
                result = true;
            else
                result = false;
            return result;
        }
    }
}