var builder = DistributedApplication.CreateBuilder(args);
Globals.Configuration = builder.Configuration;

var dbRootPassword = builder.AddParameter("db-root-password", secret: true);
var dbUserPassword = builder.AddParameter("db-user-password", secret: true);

var db = builder.AddChikExamsDb(dbRootPassword, dbUserPassword);
var qst = builder.AddChikExamsApp(db, dbRootPassword, dbUserPassword);

builder.Build().Run();
