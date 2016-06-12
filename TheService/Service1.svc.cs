using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

namespace TheService
{
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeContract GetDataUsingDataContract(CompositeContract composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }

            //string filePath = GetLogFilePath();
            //using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 0x2000, FileOptions.SequentialScan))
            //using (var xdw = XmlDictionaryWriter.CreateTextWriter(fileStream, Encoding.UTF8, false))
            //{
            //    OperationContext.Current
            //        .RequestContext
            //        .RequestMessage
            //        .CreateBufferedCopy(int.MaxValue)
            //        .CreateMessage()
            //        .WriteMessage(xdw);

            //    xdw.Flush();
            //}

            if (composite.Data.BoolValue)
            {
                composite.Data.StringValue += "Suffix";
            }

            return composite;
        }

        private static string GetLogFilePath()
        {
            return System.IO.Path.Combine("Z:\\Logs", "svcreq-" + Guid.NewGuid().ToString("N") + ".xml");
        }
    }
}
