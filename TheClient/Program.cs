using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace TheClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // ValidateSoapMessage(); return;
            //

            using (var client = new ServiceReference1.Service1Client())
            {
                var d = new
                ServiceReference1.CompositeType
                {
                    BoolValue = true,
                    StringValue = "FUBAR"
                };

                client.GetDataUsingDataContract(ref d);

                Console.Out.WriteLine(d.StringValue);
            }

            //MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10
            //MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        }

        static void ValidateSoapMessage()
        {
            string xmlpath = @"Z:\Logs\req-cc95d58adc5f4e0395765334d2853fbb.xml";

            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(xmlpath);

            var signedXml = new WSSecuritySignedXml(xmlDoc);
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
            signedXml.LoadXml((XmlElement)nodeList[0]);

            bool isValid = signedXml.CheckSignature(LoadKey("57928524B39B2D767E19212CF87DD59074276102"), verifySignatureOnly: true);

            Console.Out.WriteLine("IsValid = {0}", isValid);
        }

        private static X509Certificate2 LoadKey(string x509Thumbprint)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection all = store.Certificates;
                X509Certificate2Collection found = null;
                try
                {
                    found = store.Certificates.Find(X509FindType.FindByThumbprint, x509Thumbprint, validOnly: true);
                    return new X509Certificate2(found[0]);
                }
                finally
                {
                    ResetAll(found);
                    ResetAll(all);
                }
            }
        }

        private static void ResetAll(X509Certificate2Collection certs)
        {
            if (certs != null && certs.Count > 0)
            {
                for (int i = 0; i < certs.Count; i++)
                {
                    certs[i].Reset();
                }

                certs.Clear();
            }
        }
    }

    public sealed class WSSecuritySignedXml : SignedXml
    {
        public WSSecuritySignedXml(XmlDocument xmlDocument)
            : base(xmlDocument)
        {
        }

        public WSSecuritySignedXml(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            XmlElement idElem = base.GetIdElement(document, idValue);

            if (idElem == null)
            {
                var nsMgr = new XmlNamespaceManager(document.NameTable);
                nsMgr.AddNamespace(Wsu.Prefix, Wsu.NamespaceUri);

                idElem = document.SelectSingleNode("//*[@" + Wsu.Prefix + ":" + Wsu.Attributes.Id + "=\"" + idValue + "\"]", nsMgr) as XmlElement;
            }

            return idElem;
        }
    }

    // WS-Security-Utility schema
    public static class Wsu
    {
        public const string Prefix = "wsu";
        public const string NamespaceUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public static class Attributes
        {
            public const string Id = "Id";
        }
    }
}
