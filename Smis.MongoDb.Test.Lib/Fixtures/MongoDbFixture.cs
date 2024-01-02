using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using Smis.MongoDb.Lib.Connection;
using Smis.MongoDb.Lib.Repositories;

namespace Smis.MongoDb.Test.Lib.Fixture;

public abstract class MongoDbFixture<TDocument> : IDisposable where TDocument : class
{

    private MongoDbRunner Runner { get; }
    private MongoClient Client { get; }
    private IMongoCollection<TDocument>? collection;
    private string? collectionName;
    private IMongoDatabase? database;
    private string databaseName = "mongodb_test";
    private readonly ILogger logger;
   

    public MongoDbFixture()
    {
        logger = CreateLogger();

        logger.LogInformation("[MongoDbFixture] - Starting MongoDbRunner");
        Runner = MongoDbRunner.Start(
            binariesSearchPatternOverride: GetBinarySearchOptionForEnv(),
            singleNodeReplSet: true);       
        Client = new MongoClient(Runner.ConnectionString);
        SetConfigurationSettings();
    }

    public abstract Dictionary<string, string> Configure();

  
    protected MongoDbConnection Connection()
    {
        return new MongoDbConnection(databaseName, Client);
    }

    private void SetConfigurationSettings()
    {
        var configurationSttings = Configure();

        string? databaseNameFromConf;
        if (configurationSttings.TryGetValue("database", out databaseNameFromConf))
            databaseName = databaseNameFromConf;

        configurationSttings.TryGetValue("collectionName", out collectionName);                        
    }

    private IMongoDatabase Database()
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            logger.LogError("[MongoDbFixture] - Database name is null or empty.");
            throw new MongoException("Database name is null or empty.");
        }

        if(database is null)
            database = Client.GetDatabase(databaseName);

        return database;
    }

    private IMongoCollection<TDocument> Collection()
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            logger.LogError("[MongoDbFixture] - Collection name is null or empty.");
            throw new MongoException("Collection name is null or empty.");
        }

        if (collection is null)
            collection = Database().GetCollection<TDocument>(collectionName);

        return collection;
    }

    public void Insert(TDocument collection)
    {
        Collection().InsertOne(collection);
    }

    public List<TDocument> FindAll()
    {
        return Collection().Find(Builders<TDocument>.Filter.Empty).ToList();
    }

    public void Reset()
    {
        Collection().DeleteManyAsync(_ => true).Wait();
    }

    public void Dispose()
    {
        Runner.Dispose();
    }



    // Mongo2go has a bug with the searchPattern variable when serching for 
    // mongod binary that is in the Mongo2Go packages:
    // When specifying the NUGET_PACKAGES environment variable on Linux, the
    // search pattern does not work because it then looks the following:

    // $NUGET_PACKAGES/packages/Mongo2Go*/*/tools/mongodb-linux*/bin
    // $NUGET_PACKAGES/packages/mongo2go/*/*/tools/mongodb-linux*/bin
    // $NUGET_PACKAGES/mongo2go/*/*/tools/mongodb-linux*/bin
    // This is 1 * too many.
    //
    // The bug fixes are in a branch that has been open since 2022:
    // https://github.com/Mongo2Go/Mongo2Go/pull/140
    // 
    // Until this branch is merged we ned to apply the fixes below. 

    private string GetBinarySearchOptionForEnv()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "tools/mongodb-macos*/bin";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "tools/mongodb-linux*/bin";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "tools\\mongodb-windows*\\bin";
        }
        else
            throw new ApplicationException("Could not identifu envinronment");
    }

    private ILogger CreateLogger()
    {
        using var factory = LoggerFactory.Create(static builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole();
        });

        return factory.CreateLogger<MongoDbFixture<TDocument>>();
    }
}





