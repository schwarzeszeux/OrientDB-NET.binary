using System.Collections.Generic;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

// syntax: 
// CREATE VERTEX [<class>] 
// [CLUSTER <cluster>] 
// [SET <field> = <expression>[,]*]

namespace Orient.Client
{
    public class OSqlCreateVertex
    {
        private SqlQuery _sqlQuery = new SqlQuery();
        private Connection _connection;
        private ODocument _document;

        public OSqlCreateVertex()
        {
        }

        internal OSqlCreateVertex(Connection connection)
        {
            _connection = connection;
        }

        #region Vertex

        public OSqlCreateVertex Vertex(string className)
        {
            _sqlQuery.Vertex(className);

            return this;
        }

        public OSqlCreateVertex Vertex<T>(T obj)
        {
            

            if (obj is ODocument)
            {
                _document = obj as ODocument;
            }
            else
            {
                _document = ODocument.ToDocument(obj);
            }

            if (string.IsNullOrEmpty(_document.OClassName))
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain OClassName value.");
            }

            //_sqlQuery.Vertex(_document.OClassName);
            //_sqlQuery.Set(_document);

            return this;
        }

        public OSqlCreateVertex Vertex<T>()
        {
            return Vertex(typeof(T).Name);
        }

        #endregion

        #region Cluster

        public OSqlCreateVertex Cluster(string clusterName)
        {
            _sqlQuery.Cluster(clusterName);

            return this;
        }

        public OSqlCreateVertex Cluster<T>()
        {
            return Cluster(typeof(T).Name);
        }

        #endregion

        #region Set

        public OSqlCreateVertex Set<T>(string fieldName, T fieldValue)
        {
            _sqlQuery.Set<T>(fieldName, fieldValue);

            return this;
        }

        public OSqlCreateVertex Set<T>(T obj)
        {
            _sqlQuery.Set(obj);

            return this;
        }

        #endregion

        #region Run

        public OVertex Run()
        {
            //CommandPayload payload = new CommandPayload();
            //payload.Type = CommandPayloadType.Sql;
            //payload.Text = ToString();
            //payload.NonTextLimit = -1;
            //payload.FetchPlan = "";
            //payload.SerializedParams = new byte[] { 0 };

            //Command operation = new Command();
            //operation.OperationMode = OperationMode.Synchronous;
            //operation.ClassType = CommandClassType.NonIdempotent;
            //operation.CommandPayload = payload;

            CreateRecord operation = new CreateRecord(_document);

            OCommandResult result = new OCommandResult(_connection.ExecuteOperation(operation));

            return result.ToSingle().To<OVertex>();
        }

        public T Run<T>() where T : class, new()
        {
            return Run().To<T>();
        }

        #endregion

        public override string ToString()
        {
            return _sqlQuery.ToString(QueryType.CreateVertex);
        }
    }
}
