using System;
using System.IO;
using Microsoft.SqlServer.Dac.Compare;
using YamlDotNet.Serialization;

namespace SqlServerSchemaChanges
{
    class Program
    {
        private static string fileDateTimeStamp;

        static void Main(string[] args)
        {
            fileDateTimeStamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            using (StreamReader reader = File.OpenText("config.yaml"))
            {
                var conf = reader.ReadToEnd();
                var deserializer = new DeserializerBuilder()
                    .Build();

                var yamldoc = deserializer.Deserialize<YamlSchema>(conf);
                RunSchemaCompare(yamldoc);
                RunRollBackCreator(yamldoc);
            }

            File.Copy("config.yaml", $"config{fileDateTimeStamp}.yaml");
        }

        private static void RunSchemaCompare(YamlSchema options)
        {
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("source", "The source .dacpac file is required");
            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("target", "The target database connection string is required");

            var sourceDatabase = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);

            var comparison = new SchemaComparison(sourceDatabase, targetDatabase);

            Console.WriteLine("Running schema comparison...");
            SchemaComparisonResult compareResult = comparison.Compare();
            string diffs = string.Empty;
            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" + Environment.NewLine;
                Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
            }

            var src = compareResult.GenerateScript(options.Comparetype).Script;
            if (src == null)
            {
                diffs += "No differences detected";
                Console.WriteLine("No differences to script");
            }

            using (StreamWriter writer = File.CreateText($"changes{fileDateTimeStamp}.sql"))
            {
                writer.Write(src);
                writer.Flush();
            }

            using (StreamWriter writer = File.CreateText($"differences_{fileDateTimeStamp}.txt"))
            {
                writer.Write(diffs);
                writer.Flush();
            }
        }

        private static void RunRollBackCreator(YamlSchema options)
        {
            // Just runs the Compare in oppsite
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("source", "The source .dacpac file is required");
            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("target", "The target database connection string is required");

            var sourceDatabase = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);

            var comparison = new SchemaComparison(targetDatabase, sourceDatabase);
            Console.WriteLine("Running rollback script creator...");
            SchemaComparisonResult compareResult = comparison.Compare();
            string diffs = string.Empty;
            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" + Environment.NewLine;
                Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
            }

            var src = compareResult.GenerateScript(options.Comparetype).Script;
            if (src == null)
            {
                diffs += "No differences detected";
                Console.WriteLine("No differences to script");
            }

            using (StreamWriter writer = File.CreateText($"rollback{fileDateTimeStamp}.sql"))
            {
                writer.Write(src);
                writer.Flush();
            }

            using (StreamWriter writer = File.CreateText($"rolls_{fileDateTimeStamp}.txt"))
            {
                writer.Write(diffs);
                writer.Flush();
            }
        }
    }
}