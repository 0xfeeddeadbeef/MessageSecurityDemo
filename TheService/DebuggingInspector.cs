using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace TheService
{
    public sealed class DebuggingInspector : IDispatchMessageInspector, IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            /*
            if (!request.IsFault)
            {
                var quotas = new XmlDictionaryReaderQuotas();
                var bodyReader = request.GetReaderAtBodyContents().ReadSubtree();

                var wrapperSettings = new XmlReaderSettings();
                wrapperSettings.CloseInput = true;
                wrapperSettings.ValidationFlags = XmlSchemaValidationFlags.None;
                wrapperSettings.ValidationType = ValidationType.None;

                var wrappedReader = XmlReader.Create(bodyReader, wrapperSettings);

                File.WriteAllText(Path.Combine("Z:\\Logs\\", Guid.NewGuid().ToString("N") + ".xml"),
                    wrappedReader.ReadOuterXml());

                var memStream = new MemoryStream();
                var xdw = XmlDictionaryWriter.CreateBinaryWriter(memStream);
                xdw.WriteNode(wrappedReader, false);
                xdw.Flush();
                memStream.Position = 0L;

                var xdr = XmlDictionaryReader.CreateBinaryReader(memStream, quotas);

                Message replacedMessage = Message.CreateMessage(request.Version, null, xdr);
                replacedMessage.Headers.CopyHeadersFrom(request.Headers);
                replacedMessage.Properties.CopyProperties(request.Properties);
                request = replacedMessage;
            }
            */

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // WARNING: DOES NOT WORK WITH SECURED BINDINGS!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);  // cannot create even buffered copy when binding is secured
            try
            {
                Message toBeLogged = buffer.CreateMessage();

                string filePath = GetLogFilePath();
                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 0x2000, FileOptions.SequentialScan))
                using (var xdw = XmlDictionaryWriter.CreateTextWriter(fileStream, Encoding.UTF8, false))
                {
                    toBeLogged.WriteMessage(xdw);
                    xdw.Flush();
                }

                request = buffer.CreateMessage();
            }
            finally
            {
                if (buffer != null)
                {
                    buffer.Close();
                }
            }

            return null;
        }

        private static string GetLogFilePath()
        {
            return System.IO.Path.Combine("Z:\\Logs", "inspector-" + Guid.NewGuid().ToString("N") + ".xml");
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public sealed class DebuggingInspectorBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new DebuggingInspector();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(DebuggingInspector);
            }
        }
    }
}
