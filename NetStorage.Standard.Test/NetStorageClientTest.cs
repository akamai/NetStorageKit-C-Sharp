// Copyright 2018 Derivco Estonia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Author: meelis.talvis@derivco.ee  (Meelis Talvis)
// Contributor: colinb@akamai.com  (Colin Bendell)
//

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
        client.Uri = await client.GetNetStorageUri("/foobar").ConfigureAwait(false);
        client.Params = NetStorageAction.Dir();
        var headers = await client.ComputeHeadersAsync().ConfigureAwait(false);
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
        var response = await client.DirAsync("/dir").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task DeleteFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DeleteAsync("/delete").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task DownloadFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DownloadAsync("/download").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetDirectoryUsage()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.DUAsync("/du").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetList()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.ListAsync("/list").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task MakeDirectory()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.MkDirAsync("/mkdir").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task ModifyTime()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.MTimeAsync("/mtime").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task QuickDelete()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.QuickDeleteAsync("/quick-delete").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task RenameFile()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.RenameAsync("/rename", "").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task RemoveDirectory()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.RmDirAsync("/rmdir").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task GetItemStat()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.StatAsync("/stat").ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }

    [Fact]
    public async Task CreateSymLink()
    {
      using (var client = new NetStorageClient(new NetStorageCredentials("www.example.com", "user1", "secret1"),
        new FooHandler()))
      {
        var response = await client.SymLinkAsync("/symlink", "/cpcode/path/to/existing/object").ConfigureAwait(false);
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
        var response = await client.UploadAsync("/upload", new FileInfo(tmpFile)).ConfigureAwait(false);
        Assert.NotNull(response);
      }
    }
  }
}