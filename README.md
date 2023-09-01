# ![RealWorld Example App](logo.png)

### [Demo](https://demo.realworld.io/)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with ASP.NET + Couchbase including CRUD operations, authentication, routing, pagination, and more.

I've gone to great lengths to adhere to the ASP.NET and Couchbase community styleguides & recommendations.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

[![CI](https://github.com/mgroves/realworld-aspnet-couchbase/actions/workflows/ci-container.yml/badge.svg)](https://github.com/mgroves/realworld-aspnet-couchbase/actions/workflows/ci-container.yml)

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
    * Users - Authorization/authentication, JWT, registration, login, anything for Users
    * Follows - Follow/unfollow
    * Articles - Articles, favorites, and tags for articles
    * ...more on the way...
  * *Extensions* folder - extension methods for ASP.NET functionality

* *Conduit.Tests* - Automated tests for Conduit.Web.
  * *Unit* folder - this is where unit tests live. The folder structure within mirrors the Conduit.Web folders as much as possible.
  * *Integration* folder - this is where integration tests live. The folder structure within mirrors the Conduit.Web folders as much as possible.
  * *Extensions* folder - this is for tests of base level extensions in the Conduit.Web project
  * *TestHelpers* folder - various helpers to make test writing faster and more readable

* *Conduit.Migrations* - Automated creation of database structures. This project uses NoSqlMigrator (see below).

This Conduit implementation is being streamed [live on Twitch](https://twitch.tv/matthewdgroves). You can follow along with the [recordings on YouTube](https://www.youtube.com/watch?v=HiRj5ntqiXk&list=PLZWwU1YVRehL0psJRk35x8evMeeGAFwBa).

# Getting started

1. Create a Couchbase Capella account / database, or install Couchbase Server (both community and enterprise edition will work).
   1. [Couchbase Capella (free trial available)](https://www.couchbase.com/products/capella/)
   2. [Couchbase Server (free downloads)](https://www.couchbase.com/downloads/?family=couchbase-server)
2. Configure Couchbase
   1. Create a "Conduit" bucket
   2. Enable database access
      1. Capella: Create Database Access with read/write permission to all collections in the Conduit bucket
      2. Couchbase Server: Create a user with read/write permission (key-value, query) to all collections in the Conduit bucket. (Or if you aren't in production, use the admin credentials)
   3. Run the migrations in the *Conduit.Migrations* project (see [NoSqlMigrator](https://github.com/mgroves/NoSqlMigrator)).
   4. (Manual alternative to step 3) Create the database objects collections, indexes, documents, indexes as described in the *comments* of the classes of the *Conduit.Migrations* project.
3. Configure Conduit.Web, Conduit.Tests and Conduit.Migrations
   1. Add User Secrets to Conduit.Tests and Conduit.Migrations, following secrets.json.template examples
   2. You should use a separate bucket for integration tests, since integration tests will run "down" migrations and destroy anything in the bucket.
4. Compile and run Conduit.Web
   1. Standard compile/run from command line/VSCode/Visual Studio/Rider should work fine
   2. You can use Postman to exercise the endpoints. [RealWorld has a Postman collection available for your convenience](https://realworld-docs.netlify.app/docs/specs/backend-specs/postman) (which is currently checked into this repo, but beware--if you have issues, fall back to the official one).
   3. You can also use the generated SwaggerUI to exercise the endpoints.

# Video Series

The creation of this implementation of Conduit is being done (mostly) on a [live coding stream](https://twitch.tv/matthewdgroves). Here are all the recordings:

An overview of the goals of this project and the Real World Conduit project, featured on the On .NET Live show: https://www.youtube.com/watch?v=DGrPQqyOpcU

| Date | Description | Full Stream | Edited Stream
|--|---|-----|-----
| 2023-06-06 | Starting the Conduit project - ASP.NET + Couchbase - Office Hours | https://www.youtube.com/watch?v=HiRj5ntqiXk | https://www.youtube.com/watch?v=3ynXWW_Vyrc
| 2023-06-08 | JWT and Mediatr | https://www.youtube.com/watch?v=O5ZTnmM4RpQ | https://www.youtube.com/watch?v=56kxkW63HOM
| 2023-06-13 | Can ChatGPT write unit tests? | https://www.youtube.com/watch?v=eK9Ab4mWU_s | 
| 2023-06-16 | Validation / Integration tests | https://www.youtube.com/watch?v=3vv37DP-iLM |
| 2023-06-20 | Get User / Update User endpoints | https://www.youtube.com/watch?v=Jgen3uyNHBQ |
| 2023-06-27 | Integration tests | https://www.youtube.com/watch?v=04WWcvSwtQg
| 2023-06-30 | Recap, Update User, Get Profile | https://www.youtube.com/watch?v=kmYFII2MKMo |
| 2023-07-07 | Get Profile, refactoring, the trouble with async | https://www.youtube.com/watch?v=CJ362YSQKSs |
| 2023-07-11 | Data Modeling / Data Access patterns | https://www.youtube.com/watch?v=dGGpmWonPWs |
| 2023-07-14 | Follow and Data Structures | https://www.youtube.com/watch?v=lh8yEyGEvps |
| 2023-08-01 | Finishing follow/unfollow | https://www.youtube.com/watch?v=-4h_a--UJEU |


[YouTube Playlist - full streams](https://www.youtube.com/playlist?list=PLZWwU1YVRehL0psJRk35x8evMeeGAFwBa)

[YouTube Playlist - edited streams](https://www.youtube.com/playlist?list=PLcspbWiU9RuvvdK38xbstocZ2rLRPBibe)