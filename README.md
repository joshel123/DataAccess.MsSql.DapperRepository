# Project: DataAccess.MsSql.DapperRepository

Simple data access layer, designed to interact with MS SQL Server 2016+.

Uses dapper for most data access transactions.  


# Supported Transactions
```csharp
Task<T> Get(long key);

Task<T> Get(int key);

Task<IEnumerable<T>> GetAll();

Task<IEnumerable<T>> GetDataWithQuery(string query, object parameters);

Task<IEnumerable<T>> GetDataWithProc(string proc, object parameters);

Task<IEnumerable<T>> GetDataByFilter(string table = "", params KeyValuePair<string, string>[] filter);

Task<IEnumerable<T>> GetDataByFilter(params KeyValuePair<string, string>[] filter);
        
Task<IEnumerable<T>> GetDataByFilter(object filter, string table = "");

Task ExecuteProc(string proc, object parameters);

Task ExecuteStatement(string statement, object parameters);

Task<long> Insert(T entity);

Task<long> Insert(IEnumerable<T> entities);

Task<bool> Delete(T entity);

Task<bool> Delete(IEnumerable<T> entities);

Task<bool> Update(T entity);

Task<bool> Update(IEnumerable<T> entities);
```

# How To Use


### Passing Connection
I like to put a class like this in the layer above the data access layer.
This layer generally manages the connection string etc. and is responcible for interacting directly with the DAL.

```csharp

public class DataAccessService<T> 
{
    private readonly string _conn;

    public DataAccessService(string connectionStr)
    {
        _conn = connectionStr;
    }


    public async Task<bool> Delete(T entity)
    {
        using (var conn = new SqlConnection(_conn))
        {
            return await new Repository<T>(conn).Delete(entity);
        }
    }

    public async Task<bool> Delete(IEnumerable<T> entities)
    {
        using (var conn = new SqlConnection(_conn))
        {
            return await new Repository<T>(conn).Delete(entities);
        }
    }

....
  
}

```

### Invoking

With this in place you can just initialize and invoke desired data access method.

```csharp
DataAccessService<YourEntity> dataService = new DataAccessService<YourEntity>(connectionString);
var resp = await dataService.GetAll();
```

### Passing parameters

There are a few ways to pass parameters to the data access service:

```csharp

//initalize service
var dataService = new DataAccessService<YourEntity>(connectionString);

//Method 1: Pass as an object
var parameters = new { parameterNameA = parameterValueA, parameterNameB = parameterValueB };
var resp = await dataService.GetDataByFilter(parameters);


//Method 2: Pass as a KeyValuePair
var parameters = new KeyValuePair<string, string>[]
{
      new KeyValuePair<string, string>("paramNameA", "paramValueA"),
      new KeyValuePair<string, string>("paramNameA", "paramValueB")
};

var resp = dataService.GetDataByFilter(parameters);


//Method 3: Dapper DynamicParameter
var parameters = new KeyValuePair<string, string>[]
{
      new KeyValuePair<string, string>("paramNameA", "paramValueA"),
      new KeyValuePair<string, string>("paramNameA", "paramValueB")
};

var dbParams = new DynamicParameters();
dbParams.Add("paramNameA", "paramValueA");
dbParams.Add("paramNameB", "paramValueB");

var resp = dataService.GetDataWithProc("storedProcedureName", dbParams);

```

  
### Decorating Entities with Attributs

See dapper / dapper.contrib documentation...
Should define the primary key and table name in the entity you are inserting.

```csharp
    [Table("dbo.YourTableName")]
    public class YourTableName 
    {
        [Key]
        public int Id { get; set; }
    
    ....
    )

```




