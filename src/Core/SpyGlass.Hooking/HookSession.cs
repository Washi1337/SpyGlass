using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SpyGlass.Hooking.Protocol;

namespace SpyGlass.Hooking
{
    public class HookSession
    {
        public event EventHandler<HookEventArgs> HookTriggered;
        
        private readonly Socket _socket;
        private readonly byte[] _header = new byte[2 * sizeof(int)];
        private readonly byte[] _buffer = new byte[1024];
        
        private readonly BlockingCollection<IMessage> _bufferedMessages = new BlockingCollection<IMessage>();
        
        public const int Timeout = 5000;

        public HookSession(RemoteProcess process, IHookParametersDetector detector)
        {
            Process = process;
            Detector = detector;

            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public RemoteProcess Process
        {
            get;
        }

        public IHookParametersDetector Detector
        {
            get;
        }

        public void Connect(EndPoint endPoint)
        {
            _socket.Connect(endPoint);
            new Thread(ReceiveLoop)
            {
                IsBackground = true
            }.Start();
        }

        public void Set(IntPtr address)
        {
            var parameters = Detector.Detect(Process, address);

            _socket.Send(MessageEncoder.EncodeMessage(new SetHookMessage(address, parameters.BytesToOverwrite,
                parameters.Fixups)));

            var message = WaitForResponse<ActionCompletedMessage>();
            if (message.ErrorCode != HookErrorCode.Success)
                throw new InvalidOperationException($"Server responded with error code {message.ErrorCode}");
        }

        public void Unset(IntPtr address)
        {
            throw new NotImplementedException();
        }

        private void ReceiveLoop()
        {
            while (true)
            {
                var message = ReceiveNextMessage();

                switch (message)
                {
                    case CallbackMessage callback:
                        OnHookTriggered(new HookEventArgs(callback.Address));
                        break;
                    default:
                        _bufferedMessages.Add(message);
                        break;
                }
            }
        }

        private IMessage ReceiveNextMessage()
        {
            using (var stream = new MemoryStream())
            {
                _socket.Receive(_header, _header.Length, SocketFlags.None);
                stream.Write(_header, 0, _header.Length);

                int length = BitConverter.ToInt32(_header, 0);

                while (stream.Length < length + _header.Length)
                {
                    int received = _socket.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);
                    stream.Write(_buffer, 0, received);
                }

                return MessageEncoder.DecodeMessage(stream.ToArray());
            }
        }

        private TMessage WaitForResponse<TMessage>()
            where TMessage : IMessage
        {
            if (!_bufferedMessages.TryTake(out var message, Timeout))
                throw new InvalidOperationException("Request timed out.");
            
            if (message is TMessage m)
                return m;
            
            throw new InvalidOperationException(
                $"Server responded with an unexpected {message.GetType()} message.");
        }

        protected virtual void OnHookTriggered(HookEventArgs e)
        {
            HookTriggered?.Invoke(this, e);
        }
    }
    
}