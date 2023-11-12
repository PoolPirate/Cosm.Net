using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosm.Net.Tx;
public interface ICosmTx
{
    public ByteString Encode();
}
