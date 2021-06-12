# SqlServerSchemaComparison
Wrote this is the middle of the night.
Insperation https://github.com/reid15/DatabaseCommon 
Couple of dotnet core programs that automate schema compares.
Creates change script.
Crates rollback script.
Uses yaml config files.

SqlServerSchemaComparison allows you to specify which databases and which objects to include using config.yaml file.


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

