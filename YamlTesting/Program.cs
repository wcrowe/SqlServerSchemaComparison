using System.IO;
using YamlDotNet.Serialization;
using YamlTesting.SqlServerSchemaComparison;

namespace YamlTesting {
    class Program {
        static void Main(string[] args) {
            var yaml = new YamlSchema();
            var yamlread = new YamlSchema();
            yaml.Comparetype = "ConnectionStrings";
            var source = new Connections();
            var target = new Connections();

            source.Name = "SourceConntion";
            source.ConnectionString =
                @"Data Source=localhost,1401;Initial Catalog=northwind;Persist Security Info=True;User ID=sa;Password=Mustang74";
            target.Name = "TargetConntion";
            target.ConnectionString =
                @"Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74";
            yaml.SourceConnectionString = source;
            yaml.TargetConnectionString = target;
            var includes = new Includes();
            includes.DatabaseName = "work";
            includes.Schema = "dbo";
            includes.ObjectType = "SqlProcedure";
            includes.ObjectName = "SelectCustomers";
            var includes2 = new Includes();
            includes2.DatabaseName = "work";
            includes2.Schema = "dbo";
            includes2.ObjectType = "Table";
            includes2.ObjectName = "Testing";
            yaml.Includes.Add(includes);
            yaml.Includes.Add(includes2);
            var serializer = new Serializer();
            var s = serializer.Serialize(yaml);
               using (StreamWriter writer = File.CreateText("config.yaml"))
                            {
                                writer.Write(s);
                                writer.Flush();
               }
               using (StreamReader reader = File.OpenText("config.yaml")) {
                   var conf = reader.ReadToEnd();
                   var deserializer = new DeserializerBuilder()
                       .Build();

                   var yamldoc = deserializer.Deserialize<YamlSchema>(conf);
               }

        }
    }
}