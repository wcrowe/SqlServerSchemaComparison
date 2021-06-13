using System.Collections.Generic;

namespace YamlTesting {
    namespace SqlServerSchemaComparison
    {
        public class YamlSchema
        {
            public YamlSchema()
            {
                includes = new List<Includes>();
                sourceConnectionString = new Connections();
                targetConnectionString = new Connections();
            }
            private string comparetype;
            private Connections sourceConnectionString;
            private Connections targetConnectionString;
            private List<Includes> includes;
            public string Comparetype { 
                get => comparetype; 
                set => comparetype = value; 
            }
            public Connections SourceConnectionString { get => sourceConnectionString; set => sourceConnectionString = value; }
            public Connections TargetConnectionString { get => targetConnectionString; set => targetConnectionString = value; }
            public List<Includes> Includes { get => includes; set => includes = value; }
        }
    }

}