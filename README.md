# SqlServerSchemaComparison
Quick and dirty written at 3:00AM on a Friday night.
Insperation https://github.com/reid15/DatabaseCommon 
Couple of dotnet core programs that automate schema compares.
Creates change script.
Creates rollback script.
Uses yaml config files.

SqlServerSchemaComparison allows you to specify which databases and which objects to include using config.yaml file.
Tested on Sql Server 2019 Linux and Sql Server 2014 SP3

Usage:

```
SqlServerSchemaComparison.exe --source file.yaml
```

* TODO
    * Yaml Cofig
        * Async
        * Comparsion Type: dacpac -> dacpac
        * Comparsion Type: dacpac -> connection string
        * Comparsion Type: connection string -> dacpac
        * Comparsion Type: connection string -> connection string
        * yaml to include object types, object type and name, or wildcard * include everything.  Works with object name only now.
        * paramaters -s source yaml -o output sql file overrides yaml SqlServerSchemaComparison.exe --source file.yaml --output changesforprod.sql

# SqlServerSchemaChanges
Program scripts all the changes for a database and scripts rollback.  Wrote this to figure out Sql Object types returned from dac compare.
