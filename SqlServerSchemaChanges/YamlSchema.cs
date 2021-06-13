using System.Collections.Generic;

namespace SqlServerSchemaChanges
{
        public class YamlSchema
        {
            public YamlSchema()
            {
                Includes = new List<Includes>();
                SourceConnectionString = new Connections();
                TargetConnectionString = new Connections();
            }

            public string Comparetype { get; set; }

            public Connections SourceConnectionString { get; set; }

            public Connections TargetConnectionString { get; set; }

            public List<Includes> Includes { get; set; }
        }
    }

