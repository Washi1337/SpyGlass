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
            [3] = typeof(CallbackMessage)
        };

        private static readonly IDictionary<Type, int> TypeToMessageId = new Dictionary<Type, int>();

        static MessageEncoder()
        {
            foreach (var entry in MessageIdToType)
                TypeToMessageId[entry.Value] = entry.Key;
        }

        public static byte[] EncodeMessage(IMessage message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0); // to-be-replaced
                
                int messageId = TypeToMessageId[message.GetType()];
                writer.Write(messageId);
                message.WriteTo(writer);
                
                stream.Position = 0;
                writer.Write((int) stream.Length - 8);
                
                return stream.ToArray();
            }
        }

        public static IMessage DecodeMessage(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                int length = reader.ReadInt32(); // not used
                int messageId = reader.ReadInt32();

                var message = (IMessage) Activator.CreateInstance(MessageIdToType[messageId]);
                message.ReadFrom(reader);
                return message;
            }
        }
    }
}