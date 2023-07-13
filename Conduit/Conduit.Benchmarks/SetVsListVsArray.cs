using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Couchbase;
using Couchbase.DataStructures;
using Couchbase.KeyValue;

namespace Conduit.Benchmarks;

[SimpleJob(RuntimeMoniker.HostProcess)]
[RPlotExporter]
public class SetVsListVsArray
{
    public class FollowTracker
    {
        public List<string> Following { get; set; }
    }

    private List<string> _allUsernames = new List<string>();
    private List<string> _usernamesToSearchFor = new List<string>();
    private IPersistentList<string> _list;
    private IPersistentSet<string> _set;
    private Random _rand;
    private string _arrayKey;
    private ICouchbaseCollection _collection;

    [GlobalSetup]
    public async Task Setup()
    {
        /*
         * The goal of this benchmark is to determine the most efficient way to add a "follow" and/or check to
         * see if a follow exists.
         *
         * A Conduit User can follow another user. The way to track this is to create a separate document (per User)
         * that contains an array of usernames being followed.
         *
         * To interact with this document for the purpose of follow/unfollow, the Couchbase SubDocument API
         * seems to be the right choice.
         *
         * The Couchbase .NET SDK contains at least 3 different approaches. They all likely use similar combinations of
         * subdocument API usage for different circumstances. Which one is most efficient for *this* case?
         *
         * * List - an abstracted single JSON document of the form [item1, item2, item3 . . . ]
         * * Set - an abstracted single JSON document of the form [item1, item2, item3 . . . ], but duplicate entries not allowed
         * * ArrayAddUnique - a non-abstracted way to add items to an array in a document of the form { "following" : [item1, item2, item3 . . .]
         * Each "item" in this case is a string (username)
         *
         * Benchmark methodology:
         * 1. Construct one of each document (list, set, array) with a preset of 200 randomly generated strings
         * 2. Pick 40 of those, and make up 10 more new ones, into a list of 50
         * 3. For 50 iterations, run one of the [Benchmark]s
         */

        _rand = new Random();
                
        // create a "following" document
        // for each: List, Set, raw array in a document

        var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
        var bucket = await cluster.BucketAsync("Benchmarks");
        _collection = await bucket.CollectionAsync("runs");

        var keyPrefix = Path.GetRandomFileName();
        _list = _collection.List<string>($"{keyPrefix}-list");
        _set = _collection.Set<string>($"{keyPrefix}-set");
        _arrayKey = $"{keyPrefix}-array";
        await _collection.InsertAsync(_arrayKey, new { following = new List<string>() });

        // with 200 randomly generated names, somewhere in the 10-20 character range
        for (var i = 0; i < 200; i++)
        {
            var username = GenerateUsername();
            _allUsernames.Add(username);
            await _list.AddAsync(username);
            await _set.AddAsync(username);
            await _collection.MutateInAsync(_arrayKey, spec =>
            {
                spec.ArrayAddUnique("following", username);
            });
        }

        // put a random subset of 50 of those names into an array, 10 of which will NOT be in any of them
        _usernamesToSearchFor = _allUsernames.OrderBy(u => _rand.NextDouble())
            .Take(40)
            .ToList();
        for(var i=0;i<10;i++)
            _usernamesToSearchFor.Add(GenerateUsername());
    }

    private string GenerateUsername()
    {
        var length = _rand.Next(10, 20);
        var username = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
        return username.Substring(0, length);
    }

    /// <summary>
    /// List - do 50 reads
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task FindInList()
    {
        // determine if a given username is in the list
        var givenUsername = _usernamesToSearchFor.OrderBy(u => _rand.NextDouble()).First();
        
        await _list.ContainsAsync(givenUsername);
    }
    
    /// <summary>
    /// Set - do 50 reads
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task FindInSet()
    {
        // determine if a given username is in the set
        var givenUsername = _usernamesToSearchFor.OrderBy(u => _rand.NextDouble()).First();
        
        await _set.ContainsAsync(givenUsername);
    }
        
    /// <summary>
    /// Array - do 50 reads
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task FindInArray()
    {
        // determine if a given username is in the set
        var givenUsername = _usernamesToSearchFor.OrderBy(u => _rand.NextDouble()).First();
        
        var doc = await _collection.GetAsync(_arrayKey);
        var result = doc.ContentAs<FollowTracker>();
        var following = result.Following;
        var inArray = following.Any(f => f == givenUsername);
    }
     
    /// <summary>
    /// Set - do 50 writes, note that duplicate entries will cause an error/exception
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    [IterationCount(50)]
    public async Task AddToSet()
    {
        await _set.AddAsync(GenerateUsername());
    }
        
    /// <summary>
    /// List - do 50 writes. Notice that there is NO locking, so there is a possible race condition introduced here
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task AddToList()
    {
        var username = GenerateUsername();
        if (!(await _list.ContainsAsync(username)))
            await _list.AddAsync(GenerateUsername());
    }

    /// <summary>
    /// Array - do 50 writes, use optimistic locking
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task AddToArrayOptimistic()
    {
        var username = GenerateUsername();

        var doc = await _collection.GetAsync(_arrayKey);
        var obj = doc.ContentAs<FollowTracker>();
        var following = obj.Following;

        if (following.Contains(username))
            return;

        await _collection.MutateInAsync(_arrayKey, spec =>
        {
            spec.ArrayAddUnique("following", username);
        }, opts =>
        {
            opts.Cas(doc.Cas);
        });
    }

    /// <summary>
    /// Array - do 50 writes, use pessimistic locking
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task AddToArrayPessimistic()
    {
        var username = GenerateUsername();
        
        var result = await _collection.GetAndLockAsync(_arrayKey,TimeSpan.FromSeconds(30));
        var obj = result.ContentAs<FollowTracker>();
        var following = obj.Following;
        
        if (following.Contains(username))
            return;
        
        await _collection.MutateInAsync(_arrayKey, spec =>
        {
            spec.ArrayAddUnique("following", username);
        }, opts =>
        {
            opts.Cas(result.Cas);
        });
    }
}