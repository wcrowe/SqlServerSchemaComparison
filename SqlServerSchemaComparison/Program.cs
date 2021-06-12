using CommandLine;
using Microsoft.SqlServer.Dac.Compare;
using System;
using System.IO;
using System.Linq;

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
        private static object src;

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunSchemaCompare);
        }

        private static void RunSchemaCompare(Options options)
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

            var sourceDatabaae = new SchemaCompareDatabaseEndpoint(options.SourceConnectionString);
            //var sourceDacpac = new SchemaCompareDacpacEndpoint(options.SourceDacPac);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(options.TargetConnectionString);

            //var comparison = new SchemaComparison(sourceDacpac, targetDatabase);
             var comparison = new SchemaComparison(sourceDatabaae, targetDatabase);
            Console.WriteLine("Running schema comparison...");
            //SchemaComparisonExcludedObjectId objid = new SchemaComparisonExcludedObjectId()
            //comparison.ExcludedSourceObjects.Add();
            SchemaComparisonResult compareResult = comparison.Compare();

            foreach (SchemaDifference diff in compareResult.Differences)
            {
                string objectType = diff.Name;
                //object name "SqlProcedure"
                string sourceObject = diff.SourceObject?.Name.ToString() ?? "null";
                string targetObject = diff.TargetObject?.Name.ToString() ?? "null";
                if (diff.TargetObject.Name.Parts.Contains("SelectCustomers"))
                {
                    Console.WriteLine($"Type: {objectType}\tSource: {sourceObject}\tTarget: {targetObject}");

                }
                else
                {
                    compareResult.Exclude(diff);

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
                var scr = compareResult.GenerateScript("northwind").Script;
                if (src == null)
                {
                    Console.WriteLine("No differences to script");
                }
                using (StreamWriter writer = System.IO.File.AppendText("changes.sql"))
                {
                    writer.Write(scr);
                    writer.Flush();
                }

                // var scriptWriter = new System.IO.StreamWriter(logFile);
            }

        }
    }
}
