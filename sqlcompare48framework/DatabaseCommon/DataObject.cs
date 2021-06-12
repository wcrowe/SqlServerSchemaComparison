using System.Data;

namespace DatabaseCommon
{
    public class DataObject
    {
        public class StoredProcParameterValue
        {
            public string ParameterName { get; set; }
            public SqlDbType ParameterDataType { get; set; }
            public string ParameterValue { get; set; }

            public StoredProcParameterValue(
                string parameterName,
                SqlDbType parameterDataType,
                string parameterValue
            )
            {
                ParameterName = parameterName;
                ParameterDataType = parameterDataType;
                ParameterValue = parameterValue;
            }
        }
    }
}
