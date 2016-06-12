using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TheService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.Sign)]
        string GetData(int value);

        [OperationContract(ProtectionLevel = System.Net.Security.ProtectionLevel.Sign)]
        CompositeContract GetDataUsingDataContract(CompositeContract composite);
    }

    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    [MessageContract()]
    public class CompositeContract
    {
        [MessageBodyMember()]
        public CompositeType Data { get; set; }
    }
}
