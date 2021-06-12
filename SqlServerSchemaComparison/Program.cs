using CommandLine;
using Microsoft.SqlServer.Dac.Compare;
using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace SqlServerSchemaComparison
{
    public class Options
    {
        //[Option('s', "source", Required = true, HelpText = "The source .dacpac file")]
        //public string? SourceDacPac { get; set; }

        [Option('s', "source", Required = true, HelpText = "The source database connection string")]
        public string? SourceConnectionString { get; set; }

        [Option('t', "target", Required = true, HelpText = "The target database connection string")]
        public string? TargetConnectionString { get; set; }
    }

    class Program
    {

        private static string fileDateTimeStamp;
        static void Main(string[] args)
        {
            fileDateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            //CommandLine.Parser.Default.ParseArguments<Options>(args)
            //    .WithParsed(RunSchemaCompare);
            using (StreamReader reader = System.IO.File.OpenText("config.yaml"))
            {
                var conf = reader.ReadToEnd();
                var deserializer = new DeserializerBuilder()
                    .Build();

                var yamldoc = deserializer.Deserialize<YamlSchema>(conf);
                //CommandLine.Parser.Default.ParseArguments<Options>(args)
                //    .WithParsed(RunSchemaCompare);
                RunSchemaCompare(yamldoc);
                RunRollBackGen(yamldoc);
            }
            File.Copy("config.yaml", $"config{ fileDateTimeStamp}.yaml");
        }

        private static void RunSchemaCompare(YamlSchema options)
        {
            //C:\Users\crowe\source\repos\NorthWind\NorthWind\bin\Debug\NorthWind.dacpac
            //  to use a dapac file stuff
            //-d  "C:\Dev\sqlschema\Northwind.dacpac" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("source", "The source .dacpac file is required");

            //  to use a dapac file stuff
            //-d  "C:\Dev\sqlschema\Northwind.dacpac" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"

            //options.SourceDacPac = @"C:\Users\crowe\source\repos\NorthWind\NorthWind\bin\Debug\NorthWind.dacpac";
            //if (options.SourceDacPac is null)
            //    throw new ArgumentNullException("source", "The source database connection string is required");

            //-s  "Data Source=localhost,1401;Initial Catalog=northwind;Persist Security Info=True;User ID=sa;Password=Mustang74" -t "Data Source=localhost,1401;Initial Catalog=work;Persist Security Info=True;User ID=sa;Password=Mustang74"

            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("target", "The target database connection string is required");

            var sourceDatabaae = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
            //var sourceDacpac = new SchemaCompareDacpacEndpoint(options.SourceDacPac);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);

            //var comparison = new SchemaComparison(sourceDacpac, targetDatabase);
            var comparison = new SchemaComparison(sourceDatabaae, targetDatabase);
            Console.WriteLine("Running schema comparison...");
            //SchemaComparisonExcludedObjectId objid = new SchemaComparisonExcludedObjectId()
            //comparison.ExcludedSourceObjects.Add();
            SchemaComparisonResult compareResult = comparison.Compare();
            string diffs = string.Empty;
;            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                //object name "SqlProcedure"
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                
                diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}"  + Environment.NewLine;
                Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
                if (diff.TargetObject != null)
                {
                    System.Collections.Generic.IEnumerable<string> enumerable()
                    {
                        var parts = diff.SourceObject.Name.Parts;
                        var includes = options.Includes;
                        foreach (var firstItem in parts)
                        {
                            foreach (var secondItem in includes)
                            {
                                if (object.Equals(firstItem, secondItem.ObjectName))
                                {
                                    yield return firstItem;
                                }
                            }
                        }
                    }

                    var query = enumerable();
                    if (query.Any())
                    {
                        diffs += $"Including: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" + Environment.NewLine;
                        Console.WriteLine($"Including: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
                        continue;
                    }
                    else
                    {
                        compareResult.Exclude(diff);

                    }
                }
            }
            //foreach (var diff in compareResult.Differences.Where(x => x.UpdateAction == SchemaUpdateAction.Delete))
            //{
            //    compareResult.Exclude(diff);
            //}
            //foreach (var diff in compareResult.Differences.Where(x => x.TargetObject.Name.Parts.Contains("SelectCustomers")))
            //{
            //    compareResult.Exclude(diff);
            //}

            if (compareResult.Differences.Count() == 0)
            {
                Console.WriteLine("No differences detected");
            }
            else
            {
                var src = compareResult.GenerateScript(options.Comparetype).Script;
                if (src == null)
                {
                    Console.WriteLine("No differences to script");
                }
                using (StreamWriter writer = System.IO.File.CreateText($"changes{fileDateTimeStamp}.sql"))
                {
                    writer.Write(src);
                    writer.Flush();
                }
  
                // var scriptWriter = new System.IO.StreamWriter(logFile);
            }

        }

        private static void RunRollBackGen(YamlSchema options)
        {
            if (options.SourceConnectionString is null)
                throw new ArgumentNullException("source", "The source .dacpac file is required");
            if (options.TargetConnectionString is null)
                throw new ArgumentNullException("target", "The target database connection string is required");

            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString.ConnectionString);
            var sourceDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString.ConnectionString);
            // Run reverse
            string diffs = string.Empty;
            var comparison = new SchemaComparison(sourceDatabase, targetDatabase);

            Console.WriteLine("Running rollback creator...");
            SchemaComparisonResult compareResult = comparison.Compare();

            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                //object name "SqlProcedure"
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                diffs += $"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}" + Environment.NewLine;
                Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
                if (diff.TargetObject != null)
                {
                    System.Collections.Generic.IEnumerable<string> enumerable()
                    {
                        var parts = diff.TargetObject.Name.Parts;
                        var includes = options.Includes;
                        foreach (var firstItem in parts)
                        {
                            foreach (var secondItem in includes)
                            {
                                if (object.Equals(firstItem, secondItem.ObjectName))
                                {
                                    yield return firstItem;
                                }
                            }
                        }
                    }

                    var query = enumerable();
                    if (query.Any())
                    {
                        Console.WriteLine($"Including: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");
                        continue;
                    }
                    else
                    {
                        compareResult.Exclude(diff);

                    }
                }
                else
                {
                    compareResult.Exclude(diff);
                }
            }

            if (compareResult.Differences.Count() == 0)
            {
                Console.WriteLine("No differences detected");
            }
            else
            {
                var src = compareResult.GenerateScript(options.Comparetype).Script;
                if (src == null)
                {
                    Console.WriteLine("No differences to script");
                }
                using (StreamWriter writer = System.IO.File.CreateText($"rollback{fileDateTimeStamp}.sql"))
                {
                    writer.Write(src);
                    writer.Flush();
                }


            }

        }

    }
}
