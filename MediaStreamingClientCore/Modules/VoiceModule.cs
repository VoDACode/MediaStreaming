using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaStreaming.Client.Core.Models;
using MediaStreaming.Core;
using NAudio.Codecs;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MediaStreaming.Client.Core.Modules
{
    public sealed class VoiceModule : Module
    {
        protected override string ModuleName => "voice";
        private WaveInEvent input = new WaveInEvent();
        private WaveOutEvent output;
        private BufferedWaveProvider bufferStream;
        private ClientWebSocket VoiceStream;
        private double sensitivity = 0.02;
        private MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        public double Sensitivity
        {
            get => sensitivity;
            set
            {
                if (value <= 0)
                    return;
                sensitivity = value;
            }
        }

        public int DeviceNumber { 
            get => input.DeviceNumber; 
            set 
            {
                input.DeviceNumber = value;
            }
        }

        public MMDeviceCollection MDevices { get => deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active); }

        public VoiceModule(string ConnectWsHost, ref Client client)
            : base(ConnectWsHost, ref client)
        {
            VoiceStream = new ClientWebSocket();
        }

        public override void Start()
        {
            //создаем поток для записи нашей речи
            input.DeviceNumber = 0;
            //определяем его формат - частота дискретизации 22000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            input.WaveFormat = new WaveFormat(32000, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
            input.DataAvailable += Voice_Input;
            input.BufferMilliseconds = 20;
            //создаем поток для прослушивания входящего звука
            output = new WaveOutEvent();
            //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
            bufferStream = new BufferedWaveProvider(new WaveFormat(32000, 1));
            //привязываем поток входящего звука к буферному потоку
            output.Init(bufferStream);
            //сокет для отправки звука

            OnReceiveData += VoiceModule_OnReceiveData;

            Socket.ConnectAsync(new Uri($"{ConnectWsRootUrl}/voice/listen?id={Client.Id}&token={Token}&room={Client.Room}"), CancellationToken.None).Wait();
            VoiceStream.ConnectAsync(new Uri($"{ConnectWsRootUrl}/voice/start?id={Client.Id}&token={Token}&room={Client.Room}"), CancellationToken.None).Wait();
            _start();
            startReadStream();
            //base.Start();
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

        private void VoiceModule_OnReceiveData(ClientWebSocket socket, byte[] buffer)
        {
            try
            {
                var decode = DecodeSamples(buffer);
                bufferStream.AddSamples(decode, 0, decode.Length);
            }
            catch
            { }
        }

        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                if (ProcessData(e, sensitivity))
                {
                    var encode = EncodeSamples(e.Buffer);
                    VoiceStream.SendAsync(encode, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private bool ProcessData(WaveInEventArgs e, double porog = 0.02)
        {
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
            return (Tr || Sum2 > porog);
        }

        private byte[] EncodeSamples(byte[] data)
        {
            byte[] encoded = new byte[data.Length / 2];
            int outIndex = 0;

            for (int n = 0; n < data.Length; n += 2)
                encoded[outIndex++] = MuLawEncoder.LinearToMuLawSample(BitConverter.ToInt16(data, n));

            return encoded;
        }

        private byte[] DecodeSamples(byte[] data)
        {
            byte[] decoded = new byte[data.Length * 2];
            int outIndex = 0;
            for (int n = 0; n < data.Length; n++)
            {
                short decodedSample = MuLawDecoder.MuLawToLinearSample(data[n]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }
            return decoded;
        }
    }
}