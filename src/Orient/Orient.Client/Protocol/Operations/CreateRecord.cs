using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.Protocol.Operations
{
    class CreateRecord : IOperation
    {
        private readonly ODocument _doc;

        public CreateRecord(ODocument doc)
        {
            _doc = doc;
        }

        public Request Request(int sessionID)
        {
            var request = new Request();
            request.DataItems.Add(new RequestDataItem() { Type = "byte", Data = BinarySerializer.ToArray((byte)OperationType.RECORD_CREATE) });
            request.DataItems.Add(new RequestDataItem() { Type = "int", Data = BinarySerializer.ToArray(sessionID) });
            // operation specific fields

            request.DataItems.Add(new RequestDataItem() {Type = "short", Data = BinarySerializer.ToArray((short) -1)});
            request.DataItems.Add(new RequestDataItem() { Type = "string", Data = BinarySerializer.ToArray(_doc.Serialize()) });
            request.DataItems.Add(new RequestDataItem() {Type = "byte", Data = new[] {(byte) 'd'}});
            request.DataItems.Add(new RequestDataItem() { Type = "byte", Data = new[] { (byte)0 } });


            return request;
        }

        public ODocument Response(Response response)
        {
            throw new NotImplementedException();
        }
    }
}
