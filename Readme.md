# WebApiClient.Core

[![.NET](https://github.com/davidyujia/WebApiClient.Core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/davidyujia/WebApiClient.Core/actions/workflows/dotnet.yml)

This is a base API client for quick create custom client.

## How to create a custom API client

```cs

public class MyApiClient : WebApiClient.Core.Json.WebApiClientBase<MyCustomErrorModel>
{
    public MyApiClient(HttpClient client) : base(client) { }

    public async Task<MyItem[]> GetItems()
    {
        return await GetAsyncBase<MyItem[]>("api/Items");
    }

    public async Task AddItem(MyItem item)
    {
        await PostAsyncBase("api/Items", item);
    }
}

```

## Register custom API client in ASP.NET Core

```cs

services.AddHttpClient<MyApiClient>(c => 
{
    c.BaseAddress = new Uri("https://localhost:5000/");
});

```

ALL SET
