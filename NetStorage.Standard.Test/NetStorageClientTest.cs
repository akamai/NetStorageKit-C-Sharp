using System.Threading.Tasks;
using NetStorage.Standard.Models;
using Xunit;

namespace NetStorage.Standard.Test
{
  public class NetStorageClientTest
  {
    [Fact]
    public async Task CheckComputedHeaders()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        client.Uri = await client.GetNetStorageUri("/foobar");
        client.Params = NetStorageAction.Dir();
        var headers = await client.ComputeHeadersAsync();
        Assert.Equal(3, headers.Count);
        Assert.NotNull(headers["X-Akamai-ACS-Action"]);
        Assert.NotNull(headers["X-Akamai-ACS-Auth-Data"]);
        Assert.NotNull(headers["X-Akamai-ACS-Auth-Sign"]);
      }
    }

    [Fact]
    public async Task GetDirDocument()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DirAsync("/dir");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task DeleteFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DeleteAsync("/delete");
        Assert.True(response);
      }
    }
  }
}