using System;
using System.IO;
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
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task DownloadFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DownloadAsync("/download");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetDirectoryUsage()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DUAsync("/du");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetList()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.ListAsync("/list");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task MakeDirectory()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.MkDirAsync("/mkdir");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task ModifyTime()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.MTimeAsync("/mtime");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task QuickDelete()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.QuickDeleteAsync("/quick-delete");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task RenameFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.RenameAsync("/rename", "");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task RemoveDirectory()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.RmDirAsync("/rmdir");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetItemStat()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.StatAsync("/stat");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task CreateSymLink()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.SymLinkAsync("/symlink", "target");
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task UploadFile()
    {
      var tmpFile = Path.GetTempPath() + Guid.NewGuid() + ".txt";
      using (var sw = new StreamWriter(tmpFile))
      {
        sw.WriteLine("Upload file unit test");
        sw.Flush();
      }

      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.UploadAsync("/upload", new FileInfo(tmpFile));
        Assert.NotNull(response);
      }
    }
  }
}