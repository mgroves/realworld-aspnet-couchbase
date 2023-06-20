# ![RealWorld Example App](logo.png)

### [Demo](https://demo.realworld.io/)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with ASP.NET + Couchbase including CRUD operations, authentication, routing, pagination, and more.

I've gone to great lengths to adhere to the ASP.NET and Couchbase community styleguides & recommendations.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

# How it works

This is a Conduit backend implementation for the [RealWorld project](https://realworld-docs.netlify.app/docs/intro). It is meant to be a working reference implementation.

The code is meant to be readable, and simple enough to follow along. If you can't, please create a GitHub issue, so the problem can be addressed!

## Main Tools

You'll need to understand at least a little bit:

* [.NET 7](https://dotnet.microsoft.com/en-us/)
* [C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
* [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) - web framework for .NET
* [Couchbase Capella (or Couchbase Server)](https://www.couchbase.com/developers/) - NoSQL database
* [Mediatr](https://github.com/jbogard/MediatR)
* Other good things to understand: BCrypt, NUnit, TestContainers.net, JSON serialization, REST APIs, dependency injection, SQL

## Project Organization

* *Conduit.Web* - This is the most important project. It's where the actual implementation is. The project is "sliced" by feature as much as possible. That is, grouping together objects with common functionality.

  * *Models* folder - this is where data access objects are kept. These objects are used only for database interaction.
  * *Sliced* folders - these contain ASP.NET Core Controller(s), Mediatr request, response, and handler classes, viewmodels, and services for the functionality of the slice.
    * User - Authorization/authentication, JWT, registration, login, anything for Users
    * ...more on the way...
  * *Extensions* folder - extension methods for ASP.NET functionality

* *Conduit.Tests* - Automated tests for Conduit.Web.
  * *Fakes* folder - this contains Fakes that may be used by unit tests.
  * *Unit* folder - this is where unit tests live. The folder structure within mirrors the Conduit.Web folders as much as possible.
  * *Integration* folder - this is where integration tests live. The folder structure within mirrors the Conduit.Web folders as much as possible.

* *Conduit.Migrations* - Automated creation of database structures. This project uses NoSqlMigrator (see below).

This Conduit implementation is being being [live on Twitch](https://twitch.tv/matthewdgroves). You can follow along with the [recordings on YouTube](https://www.youtube.com/watch?v=HiRj5ntqiXk&list=PLZWwU1YVRehL0psJRk35x8evMeeGAFwBa).

# Getting started

1. Create a Couchbase Capella account / database, or install Couchbase Server (both community and enterprise edition will work).
   1. [Couchbase Capella (free trial available)](https://www.couchbase.com/products/capella/)
   2. [Couchbase Server (free downloads)](https://www.couchbase.com/downloads/?family=couchbase-server)
2. Configure Couchbase
   1. Create a "Conduit" bucket
   2. Enable database access
      1. Capella: Create Database Access with read/write permission to all collections in the Conduit bucket
      2. Couchbase Server: Create a user with read/write permission (key-value, query) to all collections in the Conduit bucket. (Or if you aren't in production, use the admin credentials)
   3. Create the following collections, indexes, documents:
      1. *Users* collection in the *_default* scope.
      2. ... more to come ...
   4. (Alternative to above step) Run the migrations in the *Conduit.Migrations* project (see [NoSqlMigrator](https://github.com/mgroves/NoSqlMigrator)).
3. Configure Conduit.Web
   1. In appsettings.json, add the Couchbase connection string, username, password, and bucket name
4. Compile and run Conduit.Web
   1. Standard compile/run from command line/VSCode/Visual Studio/Rider should work fine
   2. For now, use Postman to exercise the endpoints. [RealWorld has a Postman collection available for your convenience](https://realworld-docs.netlify.app/docs/specs/backend-specs/postman) (which is currently checked into this repo, but beware--if you have issues, fall back to the official one).

