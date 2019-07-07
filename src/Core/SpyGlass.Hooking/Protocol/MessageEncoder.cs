using System;
using System.Collections.Generic;
using System.IO;

namespace SpyGlass.Hooking.Protocol
{
    public static class MessageEncoder
    {
        private static readonly IDictionary<int, Type> MessageIdToType = new Dictionary<int, Type>
        {
            [1] = typeof(ActionCompletedMessage),
            [2] = typeof(SetHookMessage),
            [3] = typeof(CallbackMessage),
            [4] = typeof(ContinueMessage),
            [5] = typeof(MemoryReadRequest),
            [6] = typeof(MemoryReadResponse),
            [7] = typeof(MemoryEditRequest)
        };

        private static readonly IDictionary<Type, int> TypeToMessageId = new Dictionary<Type, int>();

        static MessageEncoder()
        {
            foreach (var entry in MessageIdToType)
                TypeToMessageId[entry.Value] = entry.Key;
        }

        public static byte[] EncodeMessage(Message message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // header
                writer.Write(0); // to-be-replaced
                writer.Write(TypeToMessageId[message.GetType()]);
                writer.Write(message.SequenceNumber);
                
                // payload
                message.WriteTo(writer);
                
                // update payload length.
                stream.Position = 0;
                writer.Write((int) stream.Length - 12);
                
                return stream.ToArray();
            }
        }

        public static Message DecodeMessage(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                // header
                int length = reader.ReadInt32();
                int messageId = reader.ReadInt32();
                int sequenceNumber = reader.ReadInt32();

                // create message
                var message = (Message) Activator.CreateInstance(MessageIdToType[messageId]);
                message.SequenceNumber = sequenceNumber;
                
                // payload
                message.ReadFrom(reader);
                
                return message;
            }
        }
    }
}