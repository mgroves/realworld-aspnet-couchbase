@echo off

rem This batch file can be used with NoSqlMigrator.Runner.exe to create stuctures in Couchbase
rem Find more information about NoSqlMigrator here: https://github.com/mgroves/NoSqlMigrator
rem If you don't want to use NoSqlMigrator, see the README file for manual steps

NoSqlMigrator.Runner.exe C:\zproj\realworld-aspnet-couchbase\Conduit\Conduit.Migrations\bin\Debug\net7.0\Conduit.Migrations.dll "couchbases://cb.<your connection string>.cloud.couchbase.com" "<database connection user>" "<database connection password>" "Conduit"