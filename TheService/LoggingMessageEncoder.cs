using System;
using System.Configuration;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Xml;

namespace TheService
{
    public sealed class LoggingMessageEncoder : MessageEncoder
    {
        private readonly MessageEncoder innerEncoder;

        public LoggingMessageEncoder(MessageEncoder innerEncoder)
        {
            if (innerEncoder == null)
            {
                throw new ArgumentNullException(nameof(innerEncoder));
            }

            this.innerEncoder = innerEncoder;
        }

        public override string ContentType
        {
            get { return this.innerEncoder.ContentType; }
        }

        public override string MediaType
        {
            get { return this.innerEncoder.MediaType; }
        }

        public override MessageVersion MessageVersion
        {
            get { return this.innerEncoder.MessageVersion; }
        }

        //private readonly XmlReaderSettings logReaderSettings = new XmlReaderSettings
        //{
        //    Async                        = false,
        //    CheckCharacters              = false,
        //    CloseInput                   = true,
        //    ConformanceLevel             = ConformanceLevel.Auto,
        //    DtdProcessing                = DtdProcessing.Ignore,
        //    IgnoreComments               = false,
        //    IgnoreProcessingInstructions = true,
        //    IgnoreWhitespace             = false,
        //    MaxCharactersInDocument      = 0x4000000,  // 64MiB
        //    MaxCharactersFromEntities    = 0x4000000,  // 64MiB
        //    ValidationFlags              = System.Xml.Schema.XmlSchemaValidationFlags.None,
        //    ValidationType               = ValidationType.None,
        //};

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            using (var logStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false, false))
            {
                string filePath = GetLogFilePath();

                // TODO: XmlReader.Create

                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 0x2000, FileOptions.SequentialScan))
                {
                    logStream.CopyTo(fileStream, 0x2000);
                    fileStream.Flush();
                }

                logStream.Position = 0L;
            }

            return this.innerEncoder.ReadMessage(buffer, bufferManager, contentType);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            using (var logStream = new MemoryStream())
            {
                stream.CopyTo(logStream, 0x2000);
                logStream.Flush();
                logStream.Position = 0L;

                string filePath = GetLogFilePath();

                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 0x2000, FileOptions.SequentialScan))
                {
                    logStream.CopyTo(fileStream, 0x2000);
                    fileStream.Flush();
                }

                logStream.Position = 0L;

                return this.innerEncoder.ReadMessage(logStream, maxSizeOfHeaders, contentType);
            }
        }

        private static string GetLogFilePath()
        {
            return Path.Combine("Z:\\Logs", "req-" + Guid.NewGuid().ToString("N") + ".xml");
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            this.innerEncoder.WriteMessage(message, stream);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            return this.innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
        }
    }

    // =================================================================================================================

    public sealed class LoggingMessageEncoderFactory : MessageEncoderFactory
    {
        private readonly LoggingMessageEncoder loggingEncoder;

        public LoggingMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory)
        {
            if (messageEncoderFactory == null)
            {
                throw new ArgumentNullException(nameof(messageEncoderFactory));
            }

            this.loggingEncoder = new LoggingMessageEncoder(messageEncoderFactory.Encoder);
        }

        public override MessageEncoder Encoder
        {
            get { return this.loggingEncoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return this.loggingEncoder.MessageVersion; }
        }
    }

    // =================================================================================================================

    public sealed class LoggingMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        private MessageEncodingBindingElement innerBindingElement;

        public LoggingMessageEncodingBindingElement()
            : this(new TextMessageEncodingBindingElement(MessageVersion.Soap12, System.Text.Encoding.UTF8))
        {
        }

        public LoggingMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)
        {
            this.innerBindingElement = messageEncoderBindingElement;
        }

        public MessageEncodingBindingElement InnerMessageEncodingBindingElement
        {
            get { return this.innerBindingElement; }
            set { this.innerBindingElement = value; }
        }

        public override MessageVersion MessageVersion
        {
            get { return this.innerBindingElement.MessageVersion; }
            set { this.innerBindingElement.MessageVersion = value; }
        }

        public override BindingElement Clone()
        {
            return new LoggingMessageEncodingBindingElement(this.innerBindingElement);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new LoggingMessageEncoderFactory(this.innerBindingElement.CreateMessageEncoderFactory());
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return innerBindingElement.GetProperty<T>(context);
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }

    // =================================================================================================================

    public sealed class LoggingMessageEncodingBindingElementExtensionElement : BindingElementExtensionElement
    {
        public override Type BindingElementType
        {
            get { return typeof(LoggingMessageEncodingBindingElement); }
        }

        [ConfigurationProperty("innerMessageEncoding", DefaultValue = "textMessageEncoding")]
        public string InnerMessageEncoding
        {
            get { return this["innerMessageEncoding"] as string; }
            set { this["innerMessageEncoding"] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            var binding = bindingElement as LoggingMessageEncodingBindingElement;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo["innerMessageEncoding"].ValueOrigin != PropertyValueOrigin.Default)
            {
                switch (this.InnerMessageEncoding)
                {
                    case "textMessageEncoding":
                        binding.InnerMessageEncodingBindingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap12, System.Text.Encoding.UTF8);
                        break;
                    case "binaryMessageEncoding":
                        binding.InnerMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
                        break;
                }
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            var bindingElement = new LoggingMessageEncodingBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }
    }

    // =================================================================================================================
}
