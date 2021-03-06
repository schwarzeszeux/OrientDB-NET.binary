﻿using System.Linq;
using Xunit;
using Orient.Client;
using System.Collections.Generic;

namespace Orient.Tests.Query
{
    
    public class TestTransactions
    {
        [Fact]
        public void TestUpdateVertex()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                ORID orid;
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class("TestVertexClass")
                        .Extends<OVertex>()
                        .Run();

                    OVertex testVertex = new OVertex();
                    testVertex.OClassName = "TestVertexClass";
                    testVertex.SetField("foo", "foo string value");
                    testVertex.SetField("bar", 12345);

                    Assert.Equal(null, testVertex.ORID);

                    database.Transaction.Add(testVertex);

                    Assert.NotNull(testVertex.ORID);
                    Assert.True(testVertex.ORID.ClusterPosition < 0);
                    Assert.Equal(-2, testVertex.ORID.ClusterPosition);

                    database.Transaction.Commit();
                    orid = testVertex.ORID;
                }

                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {

                    OVertex v = database.Load.ORID(orid).Run().To<OVertex>();
                    v.SetField("foobar", "blah");
                    database.Transaction.Update(v);

                    database.Transaction.Commit();
                }

                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {

                    OVertex v = database.Load.ORID(orid).Run().To<OVertex>();
                    Assert.Equal("blah", v.GetField<string>("foobar"));
                }

            }
        }

        [Fact]
        public void TestCreateVertex()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class("TestVertexClass")
                        .Extends<OVertex>()
                        .Run();

                    OVertex testVertex = new OVertex();
                    testVertex.OClassName = "TestVertexClass";
                    testVertex.SetField("foo", "foo string value");
                    testVertex.SetField("bar", 12345);

                    Assert.Equal(null, testVertex.ORID);

                    database.Transaction.Add(testVertex);

                    Assert.NotNull(testVertex.ORID);
                    Assert.True(testVertex.ORID.ClusterPosition < 0);
                    Assert.Equal(-2, testVertex.ORID.ClusterPosition);

                    database.Transaction.Commit();

                    Assert.NotNull(testVertex.ORID);
                    Assert.Equal(database.GetClusterIdFor("TestVertexClass"), testVertex.ORID.ClusterId);

                    var createdVertex = database.Load.ORID(testVertex.ORID).Run().To<OVertex>();

                    Assert.True(createdVertex.ORID != null);
                    Assert.Equal("TestVertexClass", createdVertex.OClassName);
                    Assert.Equal("foo string value", createdVertex.GetField<string>("foo"));
                    Assert.Equal(12345, createdVertex.GetField<int>("bar"));
                }

                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {


                    OVertex testVertex = new OVertex();
                    testVertex.OClassName = "TestVertexClass";
                    testVertex.SetField("foo", "foo string value");
                    testVertex.SetField("bar", 12345);

                    Assert.Equal(null, testVertex.ORID);

                    database.Transaction.Add(testVertex);

                    Assert.NotNull(testVertex.ORID);
                    Assert.True(testVertex.ORID.ClusterPosition < 0);
                    Assert.Equal(-2, testVertex.ORID.ClusterPosition);

                    database.Transaction.Commit();

                    Assert.NotNull(testVertex.ORID);
                    Assert.Equal(database.GetClusterIdFor("TestVertexClass"), testVertex.ORID.ClusterId);
                    Assert.NotEqual(-2, testVertex.ORID.ClusterPosition);

                    var createdVertex = database.Load.ORID(testVertex.ORID).Run().To<OVertex>();

                    Assert.True(createdVertex.ORID != null);
                    Assert.Equal(createdVertex.OClassName, "TestVertexClass");
                    Assert.Equal(createdVertex.GetField<string>("foo"), "foo string value");
                    Assert.Equal(createdVertex.GetField<int>("bar"), 12345);
                }

            }
        }

        [Fact]
        public void TestCreateManyVertices()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class("TestVertexClass")
                        .Extends<OVertex>()
                        .Run();

                    for (int i = 0; i < 1000; i++)
                    {
                        OVertex testVertex = new OVertex();
                        testVertex.OClassName = "TestVertexClass";
                        testVertex.SetField("foo", "foo string value");
                        testVertex.SetField("bar", i);
                        database.Transaction.Add(testVertex);
                    }

                    database.Transaction.Commit();


                    var createdVertices = database.Select().From("V").ToList();
                    Assert.Equal(1000, createdVertices.Count);

                    for (int i = 0; i < 1000; i++)
                    {
                        Assert.Equal(i, createdVertices[i].GetField<int>("bar"));
                    }

                }
            }
        }


        [Fact]
        public void TestCreateVerticesAndEdge()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class("TestVertexClass")
                        .Extends<OVertex>()
                        .Run();


                    var testVertex1 = CreateTestVertex(1);
                    var testVertex2 = CreateTestVertex(2);
                    database.Transaction.Add(testVertex1);
                    database.Transaction.Add(testVertex2);
                    testVertex1.OutE.Add(testVertex2.ORID);
                    testVertex2.InE.Add(testVertex1.ORID);


                    database.Transaction.Commit();

                    Assert.Equal(testVertex2.ORID, testVertex1.OutE.First());
                    Assert.Equal(testVertex1.ORID, testVertex2.InE.First());

                    var createdVertices = database.Select().From("V").ToList<OVertex>();
                    Assert.Equal(2, createdVertices.Count);

                    Assert.Equal(createdVertices[1].ORID, createdVertices[0].OutE.First());
                    Assert.Equal(createdVertices[0].ORID, createdVertices[1].InE.First());

                }
            }
        }

        [Fact]
        public void TestCreateVerticesAndHeavyweightEdge()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class<TestVertexClass>()
                        .Extends<OVertex>()
                        .Run();
                    database
                        .Create.Class<TestEdgeClass>()
                        .Extends<OEdge>()
                        .Run();

                    var testVertex1 = CreateTestVertex(1);
                    var testVertex2 = CreateTestVertex(2);
                    var testEdge = new TestEdgeClass();
                    testEdge.SetField("item", 1);

                    database.Transaction.Add(testVertex1);
                    database.Transaction.Add(testVertex2);
                    database.Transaction.AddEdge(testEdge, testVertex1, testVertex2);

                    Assert.Equal(testVertex1.ORID, testEdge.OutV);
                    Assert.Equal(testVertex2.ORID, testEdge.InV);

                    database.Transaction.Commit();

                    var createdVertex1 = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var createdVertex2 = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();

                    var createdEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().First();
                    Assert.Equal(createdEdge.OutV, createdVertex1.ORID);
                    Assert.Equal(createdEdge.InV, createdVertex2.ORID);

                    var testEdge2 = new TestEdgeClass();
                    testEdge2.SetField("item", 2);
                    database.Transaction.AddEdge(testEdge2, createdVertex2, createdVertex1);
                    database.Transaction.Commit();

                    createdVertex1 = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    createdVertex2 = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();
                    var createdEdge2 = database.Select().From("E").Where("item").Equals(2).ToList<OEdge>().First();

                    Assert.Equal(createdEdge2.OutV, createdVertex2.ORID);
                    Assert.Equal(createdEdge2.InV, createdVertex1.ORID);
                }
            }
        }

        /// <summary>
        /// Tests the DeleteEdge method of the OTransaction object.
        /// </summary>
        [Fact]
        public void DeleteEdgeTest()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                // Arrange
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    database
                        .Create.Class<TestVertexClass>()
                        .Extends<OVertex>()
                        .Run();
                    database
                        .Create.Class<TestEdgeClass>()
                        .Extends<OEdge>()
                        .Run();

                    var vOut = CreateTestVertex(1);
                    var vIn = CreateTestVertex(2);
                    var edge = new TestEdgeClass();
                    edge.SetField("item", 1);

                    database.Transaction.Add(vOut);
                    database.Transaction.Add(vIn);
                    database.Transaction.AddEdge(edge, vOut, vIn);

                    database.Transaction.Commit();

                    // Validate arrange
                    var createdVertex1 = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var createdVertex2 = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();
                    var createdEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().First();
                    Assert.Equal(createdEdge.OutV, createdVertex1.ORID);
                    Assert.Equal(createdEdge.InV, createdVertex2.ORID);
                }

                // Act
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    var createdEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().First();

                    database.Transaction.DeleteEdge(createdEdge);
                    database.Transaction.Commit();
                }

                // Assert
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {

                    var vOut = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var vIn = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();

                    var deletedEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().FirstOrDefault();

                    Assert.Null(deletedEdge);
                    Assert.Equal(vOut.GetField<HashSet<object>>("out_TestEdgeClass").Count, 0);
                    Assert.Equal(vIn.GetField<HashSet<object>>("in_TestEdgeClass").Count, 0);
                }
            }
        }

        /// <summary>
        /// Tests that calling delete on an edge updates the in_ and out_ properties of the end verticles (inV and outV) references too.
        /// </summary>
        [Fact]
        public void TestThatCallingDeleteOnAnEdgeUpdatesTheInAndOutReferencesToo()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                // Arrange
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    database
                        .Create.Class<TestVertexClass>()
                        .Extends<OVertex>()
                        .Run();
                    database
                        .Create.Class<TestEdgeClass>()
                        .Extends<OEdge>()
                        .Run();

                    var vOut = CreateTestVertex(1);
                    var vIn = CreateTestVertex(2);
                    var edge = new TestEdgeClass();
                    edge.SetField("item", 1);

                    database.Transaction.Add(vOut);
                    database.Transaction.Add(vIn);
                    database.Transaction.AddEdge(edge, vOut, vIn);

                    database.Transaction.Commit();

                    // Validate arrange
                    var createdVertex1 = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var createdVertex2 = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();
                    var createdEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().First();
                    Assert.Equal(createdEdge.OutV, createdVertex1.ORID);
                    Assert.Equal(createdEdge.InV, createdVertex2.ORID);
                }

                // Act
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    var createdEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().First();

                    database.Transaction.Delete(createdEdge);
                    database.Transaction.Commit();
                }

                // Assert
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {

                    var vOut = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var vIn = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();

                    var deletedEdge = database.Select().From("E").Where("item").Equals(1).ToList<OEdge>().FirstOrDefault();

                    Assert.Null(deletedEdge);
                    Assert.Equal(vOut.GetField<HashSet<object>>("out_TestEdgeClass").Count, 0);
                    Assert.Equal(vIn.GetField<HashSet<object>>("in_TestEdgeClass").Count, 0);
                }
            }
        }

        /// <summary>
        /// Tests that the edge rids are added to the Verticles edge lists (in_... and out_ ... fields)
        /// when the edge list is not empty on the verticle.
        /// </summary>
        [Fact]
        public void TestAddEdgeToVerticlaWhichAlreadyHasEdges()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database
                        .Create.Class<TestVertexClass>()
                        .Extends<OVertex>()
                        .Run();
                    database
                        .Create.Class<TestEdgeClass>()
                        .Extends<OEdge>()
                        .Run();

                    var fromV = CreateTestVertex(1);
                    var toV = CreateTestVertex(2);
                    var firstEdge = new TestEdgeClass();
                    firstEdge.SetField("item", 1);

                    database.Transaction.Add(fromV);
                    database.Transaction.Add(toV);
                    database.Transaction.AddEdge(firstEdge, fromV, toV);

                    Assert.Equal(fromV.ORID, firstEdge.OutV);
                    Assert.Equal(toV.ORID, firstEdge.InV);

                    database.Transaction.Commit();
                }
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    var fromV = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var toV = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();

                    var secondEdge = new TestEdgeClass();
                    secondEdge.SetField("item", 2);
                    database.Transaction.AddEdge(secondEdge, fromV, toV);
                    database.Transaction.Commit();

                }
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    var fromV = database.Select().From("V").Where("bar").Equals(1).ToList<OVertex>().First();
                    var toV = database.Select().From("V").Where("bar").Equals(2).ToList<OVertex>().First();
                    var secondEdge = database.Select().From("E").Where("item").Equals(2).ToList<OEdge>().First();

                    Assert.Equal(secondEdge.OutV, fromV.ORID);
                    Assert.Equal(secondEdge.InV, toV.ORID);
                    Assert.Equal(fromV.GetField<HashSet<object>>("out_TestEdgeClass").Count, 2);
                    Assert.Equal(toV.GetField<HashSet<object>>("in_TestEdgeClass").Count, 2);
                }
            }
        }

        class Widget : OBaseRecord
        {
            public string Foo { get; set; }
            public int Bar { get; set; }
            public ORID OtherWidget { get; set; }
        }

        [Fact]
        public void TestTypedCreateVerticesAndLinks()
        {
            using (TestDatabaseContext testContext = new TestDatabaseContext())
            {
                using (ODatabase database = new ODatabase(TestConnection.GlobalTestDatabaseAlias))
                {
                    // prerequisites
                    database.Create.Class<Widget>().Extends<OVertex>().Run();


                    var w1 = new Widget() { Foo = "foo", Bar = 1 };
                    var w2 = new Widget() { Foo = "woo", Bar = 2 };

                    database.Transaction.Add(w1);
                    database.Transaction.Add(w2);
                    w1.OtherWidget = w2.ORID;

                    database.Transaction.Commit();

                    Assert.Equal(w2.ORID, w1.OtherWidget);

                    var createdVertices = database.Select().From<Widget>().ToList<Widget>();
                    Assert.Equal(2, createdVertices.Count);

                    var withLink = createdVertices.First(x => x.OtherWidget != null);
                    var noLink = createdVertices.First(x => x.OtherWidget == null);


                    Assert.Equal(noLink.ORID, withLink.OtherWidget);

                }
            }
        }

        private static OVertex CreateTestVertex(int iBar)
        {
            OVertex testVertex = new OVertex();
            testVertex.OClassName = "TestVertexClass";
            testVertex.SetField("foo", "foo string value");
            testVertex.SetField("bar", iBar);
            return testVertex;
        }
    }
}
