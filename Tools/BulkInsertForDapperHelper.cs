using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace DataAccess.MsSql.DapperRepository.Tools
{
    internal static class BulkInsertForDapperHelper
    {
        private static bool IsPropertyMarkedComputed(PropertyDescriptor propertyDescriptor) =>
            propertyDescriptor.Attributes.OfType<ComputedAttribute>().Any();

        private static bool IsPropertyMarkedKey(PropertyDescriptor propertyDescriptor) =>
            propertyDescriptor.Attributes.OfType<KeyAttribute>().Any();


        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var propsValidForInsert = new List<PropertyDescriptor>();

            //get the properties that will be needed for inserts
            //basically all those without 'computed' or key attributes
            foreach (PropertyDescriptor prop in properties)
                if (!IsPropertyMarkedComputed(prop) && !IsPropertyMarkedKey(prop))
                    propsValidForInsert.Add(prop);

            //add only the valid properties to the data table
            foreach (var prop in propsValidForInsert)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            //construct the data table
            foreach (T item in data)
            {
                var row = table.NewRow();
                foreach (var prop in propsValidForInsert)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }


        /// <summary>
        /// Gets the rows copied from the specified SqlBulkCopy object
        /// </summary>
        /// <param name="bulkCopy">The bulk copy.</param>
        /// <returns></returns>
        public static bool TryGetRowsCopied(this SqlBulkCopy bulkCopy, out int rowsCopied)
        {
            FieldInfo rowsCopiedField = null;
            if (rowsCopiedField == null)
            {
                rowsCopiedField = typeof(SqlBulkCopy).GetField("_rowsCopied", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            }

            return int.TryParse(rowsCopiedField?.GetValue(bulkCopy).ToString(), out rowsCopied);
        }


    }
}
