using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DoctorProxy.Data
{
    public class ConstraintIsNull : Attribute
    {
        public string DataSourceName { get; set; }

        public ConstraintIsNull(string sourceName)
        {
            DataSourceName = sourceName;
        }
    }

    public class ColumnList
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class ColumnData
    {
        public string Name { get; set; }
        public string NameSpace { get; set; }

        public ColumnData()
        {
            Name = "Root";
            NameSpace = "Root";
        }

        public ColumnData(string nameSet, string nameSpaceSet)
        {
            Name = nameSet;
            NameSpace = nameSpaceSet;
        }

        public override bool Equals(object obj)
        {
            var target = (ColumnData)obj;
            return target.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }
    }



    public static class DataTableExtensions
    {
        public static List<T> ToListOf<T>(this DataTable table) where T : new()
        {
            return ToListOf<T>(table, null);
        }

        public static List<T> ToListOf<T>(this DataTable table, Func<T, DataRow, T> callback) where T : new()
        {
            var result = new List<T>();

            if (table.Rows.Count > 0)
            {
                var targetType = typeof(T);
                var columns = CreatePropertyTree(GetColumnList(table));
                var rows = table.AsEnumerable().ToList();
                //var objectFields = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new
                //{
                //    Name = p.Name,
                //    Type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType
                //}).ToList();

                result.AddRange(from row in rows let obj = SetProperty(row, targetType, columns.Children) select (callback != null) ? callback((T)obj, row) : (T)obj);
            }

            return result;
        }


        public static T ToObjectOf<T>(this DataTable table) where T : new()
        {
            return ToObjectOf<T>(table, null);
        }

        public static T ToObjectOf<T>(this DataTable table, Func<T, T> callback) where T : new()
        {
            T result = default(T);

            if (table.Rows.Count > 0)
            {
                var targetType = typeof(T);
                var columns = CreatePropertyTree(GetColumnList(table));

                result = (T)SetProperty(table.Rows[0], targetType, columns.Children);
            }

            if (callback != null && result != null)
                return callback(result);

            return result;
        }

        private static List<ColumnList> GetColumnList(DataTable table)
        {
            return table.Columns.Cast<DataColumn>().Select(c => new ColumnList
            {
                Name = c.ColumnName,
                Type = c.DataType
            }).ToList();
        }


        public static TreeNode<ColumnData> CreatePropertyTree(List<ColumnList> cols)
        {
            var tree = new TreeNode<ColumnData>(new ColumnData());

            foreach (var col in cols)
            {
                var nodes = col.Name.Split('.');
                var root = tree;
                for (int i = 0; i < nodes.Length; i++)
                {
                    string sourceCol = "";

                    for (int j = 0; j <= i; j++)
                    {
                        var dot = (j == i) ? "" : ".";
                        sourceCol += nodes[j] + dot;
                    }

                    root = root.AddChild(new ColumnData(nodes[i], sourceCol));
                }
            }

            return tree;
        }

        private static object SetProperty(DataRow row, Type type, IEnumerable<TreeNode<ColumnData>> nodes)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var child in nodes)
            {
                var name = child.Data.Name;
                var colName = child.Data.NameSpace;

                var field = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    if (row.Table.Columns.Contains(colName))
                    {
                        var value = row[colName];
                        if (value is Guid)
                        {
                            field.SetValue(obj, CastGuidToString(value));
                        }
                        else if (field.PropertyType.IsPrimitive ||
                            field.PropertyType == typeof(decimal) ||
                            field.PropertyType == typeof(string) ||
                            field.PropertyType == typeof(DateTime) ||
                            field.PropertyType == typeof(TimeSpan) ||
                            (field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            )
                        {
                            var castedValue = Cast(value, field.PropertyType);
                            field.SetValue(obj, castedValue);
                        }
                        else if (field.PropertyType.IsEnum)
                        {
                            var castedValue = Enum.ToObject(field.PropertyType, value);
                            field.SetValue(obj, castedValue);
                        }
                    }
                    else
                    {
                        var attr = field.GetCustomAttribute<ConstraintIsNull>();
                        if (attr != null)
                        {
                            if (!row.Table.Columns.Contains(attr.DataSourceName) ||
                                row[attr.DataSourceName] is DBNull) continue;
                            var complex = SetProperty(row, field.PropertyType, child.Children);
                            field.SetValue(obj, complex);
                        }
                        else
                        {
                            var complex = SetProperty(row, field.PropertyType, child.Children);
                            field.SetValue(obj, complex);
                        }
                    }
                }
            }

            return obj;
        }


        /*
                private static dynamic Cast(object obj)
                {
                    if (obj == null || obj is DBNull)
                        return null;

                    var objType = obj.GetType();
                    return Cast(obj, objType);
                }
        */

        private static dynamic Cast(object obj, Type castTo)
        {
            if (obj == null || obj is DBNull)
                return null;

            if (castTo == null)
                throw new ArgumentNullException("castTo");

            if (!castTo.IsGenericType || castTo.GetGenericTypeDefinition() != typeof(Nullable<>))
                return Convert.ChangeType(obj, castTo);
            var nullableConverter = new NullableConverter(castTo);
            castTo = nullableConverter.UnderlyingType;
            return Convert.ChangeType(obj, castTo);
        }

        private static string CastGuidToString(object data)
        {
            Guid result;
            if (!Guid.TryParse(data.ToString(), out result)) return null;
            var value = result.ToString();
            return value;
        }
    }


}