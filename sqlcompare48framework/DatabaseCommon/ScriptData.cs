using Microsoft.SqlServer.Management.Smo;
using System;
using System.Text;

namespace DatabaseCommon
{
    public static class ScriptData
    {
        public static string EscapeStringForSql(
            string inputString
        )
        {
            return inputString.Replace("'", "''");
        }

        public static string DelimitData(
            object data,
            Column column
        )
        {
            if (data == null || data == DBNull.Value)
            {
                return "NULL";
            }
            string dataType = column.DataType.Name;
            if (dataType == "bit")
            {
                if ((bool)data)
                {
                    return "1";
                }
                return "0";
            }
            if (dataType == "binary" || dataType == "varbinary")
            {
                var binaryArray = (byte[])data;
                var returnString = new StringBuilder();
                returnString.Append("0x");
                foreach (var item in binaryArray)
                {
                    returnString.Append(item.ToString("X2"));
                }
                return returnString.ToString();
            }
            if (dataType == "datetime")
            {
                var dateData = (DateTime)data;
                return "'" + dateData.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }
            if (dataType == "date")
            {
                var dateData = (DateTime)data;
                return "'" + dateData.ToString("yyyy-MM-dd") + "'";
            }
            string stringData = Convert.ToString(data);
            if (DatabaseSchema.IsNumericDataType(column))
            {
                return stringData;
            }

            return "'" + EscapeStringForSql(stringData) + "'";
        }
    }
}
