using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new ServiceReference1.Service1Client())
            {
                var rsp = client.GetDataUsingDataContract(new ServiceReference1.CompositeType
                {
                    BoolValue = true,
                    StringValue = "FUBAR"
                });

                Console.Out.WriteLine(rsp.StringValue);
            }
        }
    }
}
