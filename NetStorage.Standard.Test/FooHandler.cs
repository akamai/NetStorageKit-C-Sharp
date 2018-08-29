using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NetStorage.Standard.Test
{
  public class FooHandler : HttpMessageHandler
  {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      var content = await GetContent(request);
      return await Task.FromResult(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new ReadOnlyMemoryContent(new ReadOnlyMemory<byte>(content.ToByteArray()))
      });
    }

    private static async Task<string> GetContent(HttpRequestMessage request)
    {
      var action = request.RequestUri.Segments[1];
      switch (action)
      {
        case "dir":
          return await GetDirContent();
        case "delete":
          return "";
        case "download":
          return "";
        default:
          return null;
      }
    }

    private static async Task<string> GetDirContent()
    {
      const string content = @"
        <stat directory=""/[CP code]/sampledir"">
            <file type=""file"" name=""File2"" size=""398421"" md5=""[HASH]"" mtime=""1524068379""/>
            <file type=""symlink"" name=""My_symlink.html"" target=""File1"" mtime=""1524110333""/>
            <file type=""dir"" name=""dir1"" bytes=""19873716"" files=""6"" mtime=""1524068415"" implicit=""true""/>
            <file type=""dir"" name=""dir2"" bytes=""3874912"" files=""1"" mtime=""1524068422"" implicit=""true""/>
            <file type=""dir"" name=""explicitdir1"" bytes=""0"" files=""1"" mtime=""1524068459""/>
            <file type=""dir"" name=""explicitdir2"" bytes=""3"" files=""2"" mtime=""1524068462""/>
            <file type=""file"" name=""file1"" size=""532459"" md5=""[HASH]"" mtime=""1524068382""/>
        </stat>";

      return await Task.FromResult(content);
    }
  }
}