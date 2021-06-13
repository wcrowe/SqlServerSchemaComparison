using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.SqlServer.Dac.Compare;
using YamlDotNet.Serialization;

namespace SqlServerSchemaComparison {
    public class Options {
        //[Option('s', "source", Required = true, HelpText = "The source .dacpac file")]
        //public string? SourceDacPac { get; set; }

        [Option('s', "source", Required = true, HelpText = "The source database connection string")]
        public string? SourceConnectionString { get; set; }

        [Option('t', "target", Required = true, HelpText = "The target database connection string")]
        public string? TargetConnectionString { get; set; }
    }

    internal static class Program {
        private static string? _fileDateTimeStamp;

        private static async Task Main(string[] args) {
            _fileDateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            //CommandLine.Parser.Default.ParseArguments<Options>(args)
            //    .WithParsed(RunSchemaCompare);
            using (StreamReader reader = File.OpenText("config.yaml")) {
                var conf = await reader.ReadToEndAsync();
                var deserializer = new DeserializerBuilder()
                    .Build();

                var yamldoc = deserializer.Deserialize<YamlSchema>(conf);
                //CommandLine.Parser.Default.ParseArguments<Options>(args)
                //    .WithParsed(RunSchemaCompare);
                await RunSchemaCompareAsync(yamldoc);
                await RunRollBackGenAsync(yamldoc);
            }

            File.Copy("config.yaml", $"config{_fileDateTimeStamp}.yaml");
        }

        private static async Task RunSchemaCompareAsync(YamlSchema options) {
            //C:\Users\crowe\source\repos\NorthWind\NorthWind\bin\Debug\NorthWind.dacpac
            //  to use a dapac file stuff
            //-d  "C:\Dev\sqlschema\Northwind.dacpac" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("options", "The source .dacpac file is required");

            //  to use a dapac file stuff
            //-d  "C:\Dev\sqlschema\Northwind.dacpac" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"

            //options.SourceDacPac = @"C:\Users\crowe\source\repos\NorthWind\NorthWind\bin\Debug\NorthWind.dacpac";
            //if (options.SourceDacPac is null)
            //    throw new ArgumentNullException("source", "The source database connection string is required");

            //-s  "Data Source=localhost,1401;Initial Catalog=northwind;Persist Security Info=True;User ID=sa;Password=Mustang74" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"

            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("options", "The target database connection string is required");

              //var sourceDacpac = new SchemaCompareDacpacEndpoint(options.SourceDacPac);
              var sourceDatabase = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
              var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);

            //var comparison = new SchemaComparison(sourceDacpac, targetDatabase);
            var comparison = new SchemaComparison(sourceDatabase, targetDatabase);
            Console.WriteLine("Running schema comparison...");
            //SchemaComparisonExcludedObjectId objid = new SchemaComparisonExcludedObjectId()
            //comparison.ExcludedSourceObjects.Add();
            SchemaComparisonResult compareResult = comparison.Compare();
            string diffs = string.Empty;
            if (compareResult.Differences != null) {
                foreach (SchemaDifference diff in compareResult.Differences) {
                    string objectType = diff.Name;
                    string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                    string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                    diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" +
                             Environment.NewLine;
                    Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
                    var parts = diff.SourceObject?.Name.Parts;
                    if (parts != null && parts.Count == 2) {
                        var schema = parts[0];
                        var searchFor = parts[1];
                        if (options.Includes.Any(x =>
                            Includes.ObjectName.ToLower() == searchFor.ToLower() &&
                            Includes.Schema.ToLower() == schema.ToLower())) { }
                        else {
                            compareResult.Exclude(diff);
                        }
                    }
                    else {
                        compareResult.Exclude(diff);
                    }
                }
            }

            var src = compareResult.GenerateScript(options.Comparetype).Script;
            if (src == null) {
                diffs += "No differences detected";
                Console.WriteLine("No differences to script");
            }

            using (StreamWriter writer = File.CreateText($"changes{_fileDateTimeStamp}.sql")) {
                await writer.WriteAsync(src);
                await writer.FlushAsync();
            }

            using (StreamWriter writer = File.CreateText($"differences_{_fileDateTimeStamp}.txt")) {
                await writer.WriteAsync(diffs);
                await writer.FlushAsync();
            }
        }

        private static async Task RunRollBackGenAsync(YamlSchema options) {
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("options", "The source .dacpac file is required");
            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("options", "The target database connection string is required");

            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
            var sourceDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);

            // Run reverse
            string diffs = string.Empty;
            var comparison = new SchemaComparison(sourceDatabase, targetDatabase);

            Console.WriteLine("Running rollback creator...");
            SchemaComparisonResult compareResult = comparison.Compare();
            if (compareResult.Differences != null) {
                foreach (SchemaDifference diff in compareResult.Differences) {
                    string objectType = diff.Name;
                    //object name "SqlProcedure"
                    string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                    string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                    diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" +
                             Environment.NewLine;
                    Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");

                    if (diff.SourceObject is null) {
                        continue;
                    }

                    var parts = diff.SourceObject.Name.Parts;
                    if (parts != null && parts.Count == 2) {
                        var schema = parts[0];
                        var searchFor = parts[1];
                        if (options.Includes.Any(x =>
                            Includes.ObjectName.ToLower() == searchFor.ToLower() &&
                            Includes.Schema.ToLower() == schema.ToLower())) { }
                        else {
                            compareResult.Exclude(diff);
                        }
                    }
                    else {
                        compareResult.Exclude(diff);
                    }
                }
            }

            var src = compareResult.GenerateScript(options.Comparetype).Script;
            if (src == null) {
                Console.WriteLine("No differences to script");
            }

            using (StreamWriter writer = File.CreateText($"rollback{_fileDateTimeStamp}.sql")) {
                await writer.WriteAsync(src);
                await writer.FlushAsync();
            }

            using (StreamWriter writer = File.CreateText($"difs_ro{_fileDateTimeStamp}.txt")) {
                await writer.WriteAsync(diffs);
                await writer.FlushAsync();
            }
        }
    }
}