using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace KSynthesizer.Cli
{
    interface IEventInput
    {
        ToneEvent Event { get; }

        bool Exit { get; }

        EventReadResult Read();
    }

    enum EventReadResult
    {
        Success,
        Timeout,
        Failed,
    }

    class TestEventInput : IEventInput
    {
        private int state = 0;

        public ToneEvent Event { get; private set; }

        public bool Exit { get; private set; } = false;

        public EventReadResult Read()
        {
            if (Event != null)
            {
                Thread.Sleep(2000);
            }
            
            Event = null;
            switch (state++)
            {
                case 6:
                    Exit = true;
                    break;
                default:
                    var tone = new IonianTone()
                    {
                        Scale = (IonianScale)state,
                        Octave = 4,
                    };
                    Console.WriteLine($"{(false ? "Release" : "Attack ")} : Scale {tone.Scale}, Octave {tone.Octave}");
                    Event = new ToneKeyEvent(tone, false);
                    break;
            }

            return EventReadResult.Success;
        }
    }

    class ManualEventInput : IEventInput
    {
        public ToneEvent Event { get; private set; }
        
        public ToneEvent NextEvent { get; set; }

        public bool Exit { get; } = false;
        
        public EventReadResult Read()
        {
            Thread.Sleep(100);
            Event = NextEvent;
            NextEvent = null;

            return EventReadResult.Success;
        }
    }

    class SerialEventInput : IEventInput, IDisposable
    {
        private readonly SerialPort serialPort = new SerialPort();

        private bool initialized = false;

        public SerialEventInput()
        {
            SerialPortNames = SerialPort.GetPortNames();
        }
        
        public ToneEvent Event { get; private set; } = null;

        public bool Exit { get; private set; } = false;
        
        public string[] SerialPortNames { get; }
        
        public string SerialPortName { get; private set; }

        public void SetPortName(string portName)
        {
            SerialPortName = portName;
        }
        
        public EventReadResult Read()
        {
            if (!initialized)
            {
                serialPort.BaudRate = 9600;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.PortName = SerialPortName;
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.Open();
                initialized = true;
            }

            int typeInt;
            do
            {
                try
                {
                    var typeChar = (char)serialPort.ReadChar();
                    typeInt = (int)char.GetNumericValue(typeChar);
                }
                catch (TimeoutException)
                {
                    return EventReadResult.Timeout;
                }
            } while (typeInt > 4);

            var type = (ToneEventType) typeInt;

            try
            {
                byte[] buffer;
                switch (type)
                {
                    case ToneEventType.Key:
                        buffer = ReadBytes(ToneKeyEvent.DataLength);
                        Event = ToneKeyEvent.ParseEvent(buffer, 0);
                        break;
                    case ToneEventType.Thumb:
                        buffer = ReadBytes(ToneThumbEvent.DataLength);
                        Event = ToneThumbEvent.ParseEvent(buffer);
                        break;
                    case ToneEventType.Filter:
                        Event = new ToneFilterEvent();
                        break;
                    case ToneEventType.Wave:
                        Event = new ToneWaveEvent();
                        break;
                }
            }
            catch (TimeoutException)
            {
                return EventReadResult.Timeout;
            }

            return EventReadResult.Success;
        }

        private byte[] ReadBytes(int length)
        {
            var buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = (byte)serialPort.ReadByte();
            }

            return buffer;
        }

        public void Dispose()
        {
            serialPort.Dispose();
        }
    }
}
