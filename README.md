# HowlDev.Web.Helpers
Contains a few helpers used often in web projects. 

## HowlDev.Web.Helpers.DbConnecter

This is a single class that has 4 methods that helps deal with thread pooling from Dapper. It is supposed to be used like the following: 

```csharp
public Task<IEnumerable<Account>> GetAllUsersAsync() =>
    conn.WithConnectionAsync(async conn => {
        var GetUsers = "select p.id, p.accountName, p.role from \"HowlDev.User\" p order by 1";
        try {
            return await conn.QueryAsync<Account>(GetUsers);
        } catch {
            return [];
        }
    }
);
```

This is a method that returns like a get call, with the important `conn.WithConnectionAsync(async conn => {...})` call. In my experience, you run into thread pool locking pretty quick, so it's best just to start by using this. 

AI happens to be pretty good at converting old calls into this format if you give it to them. 
