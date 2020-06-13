using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class DataAdapterDataAttributeTests
    {
        [Fact]
        public void WillConvertDBNullToNull()
        {
            DataAdapterDataAttribute attr = new TestableDataAdapterDataAttribute(DBNull.Value);

            List<object[]> results = new List<object[]>(attr.GetData(null, new Type[] { typeof(object) }));

            object[] result = Assert.Single(results);
            object singleResult = Assert.Single(result);
            Assert.Null(singleResult);
        }

        [Fact]
        public void WillNotThrowWhenGivenInsufficientParameterTypeLength()
        {
            DataAdapterDataAttribute attr = new TestableDataAdapterDataAttribute(DBNull.Value);

            Assert.DoesNotThrow(() => new List<object[]>(attr.GetData(null, new Type[0])));
        }
    }

    class TestableDataAdapterDataAttribute : DataAdapterDataAttribute
    {
        readonly object[] data;

        public TestableDataAdapterDataAttribute(params object[] data)
        {
            this.data = data;
        }

        protected override IDataAdapter DataAdapter
        {
            get { return new InlineDataAdapter(data); }
        }

        class InlineDataAdapter : IDataAdapter
        {
            readonly object[] data;

            public InlineDataAdapter(object[] data)
            {
                this.data = data;
            }

            public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
            {
                throw new NotImplementedException();
            }

            public int Fill(DataSet dataSet)
            {
                DataTable table = dataSet.Tables.Add();

                foreach (object value in data)
                    table.Columns.Add(new DataColumn());

                table.Rows.Add(data);
                return 1;
            }

            public IDataParameter[] GetFillParameters()
            {
                throw new NotImplementedException();
            }

            public int Update(DataSet dataSet)
            {
                throw new NotImplementedException();
            }

            public MissingMappingAction MissingMappingAction
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public MissingSchemaAction MissingSchemaAction
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public ITableMappingCollection TableMappings
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
