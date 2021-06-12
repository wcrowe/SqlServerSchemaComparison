namespace SqlServerSchemaComparison {
    public class Connections
    {
        private string name;
        private string connectionString;

        public string Name { get => name; set => name = value; }
        public string ConnectionString { get => connectionString; set => connectionString = value; }
    }
}