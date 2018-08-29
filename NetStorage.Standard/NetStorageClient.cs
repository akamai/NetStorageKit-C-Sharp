// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NetStorage.Standard.Models;
using Polly;

namespace NetStorage.Standard
{
  public class NetStorageClient : HttpClient
  {
    public Uri Uri { get; set; }
    public const string ActionHeader = "X-Akamai-ACS-Action";
    public const string AuthDataHeader = "X-Akamai-ACS-Auth-Data";
    public const string AuthSignHeader = "X-Akamai-ACS-Auth-Sign";

    public NetStorageCredentials Credentials { get; set; }
    public SignType SignVersion { get; set; }
    public APIParams Params { get; set; }

    public NetStorageClient(NetStorageCredentials credentials)
    {
      SignVersion = SignType.HMACSHA256;
      Credentials = credentials;
    }

    public NetStorageClient(NetStorageCredentials credentials, HttpMessageHandler handler) : base(handler)
    {
      SignVersion = SignType.HMACSHA256;
      Credentials = credentials;
    }

    public async Task<Uri> GetNetStorageUri(string path)
    {
      return await Task.FromResult(
        new UriBuilder {Scheme = Credentials.UseSSL ? "HTTPS" : "HTTP", Host = Credentials.HostName, Path = path}
          .Uri);
    }

    public async Task<string> CreateActionHeader()
    {
      return await Task.FromResult(Params.ConvertToQueryString(Signer.ParamsNameFormatter,
        Signer.ParamsValueFormatter));
    }

    public async Task<string> CreateAuthDataHeader()
    {
      return await Task.FromResult(
        $"{SignVersion.VersionID}, 0.0.0.0, 0.0.0.0, {DateTime.UtcNow.GetEpochSeconds()}, {new Random().Next()}, {Credentials.Username}");
    }

    public async Task<string> CreateAuthSignHeader(string action, string authData)
    {
      var signData = $"{authData}{Uri.AbsolutePath}\n{ActionHeader.ToLower()}:{action}\n".ToByteArray();

      return await Task.FromResult(signData.ComputeKeyedHash(Credentials.Key, SignVersion.Algorithm).ToBase64());
    }

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      var headers = await ComputeHeadersAsync();
      foreach (var header in headers)
      {
        request.Headers.Add(header.Key, header.Value);
      }

      return await base.SendAsync(request, cancellationToken);
    }

    public async Task<Dictionary<string, string>> ComputeHeadersAsync()
    {
      var action = await CreateActionHeader();
      var authData = await CreateAuthDataHeader();
      var authSign = await CreateAuthSignHeader(action, authData);

      return await Task.FromResult(new Dictionary<string, string>
      {
        {ActionHeader, action},
        {AuthDataHeader, authData},
        {AuthSignHeader, authSign}
      });
    }

    public new async Task<bool> DeleteAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Delete;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Put, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<string> DirAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Dir();

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Get, Uri), CancellationToken.None));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<Stream> DownloadAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Download;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Get, Uri), CancellationToken.None));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStreamAsync();
      }

      return null;
    }

    public async Task<string> DUAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.DU();

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Get, Uri), CancellationToken.None));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<string> ListAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.List();

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Get, Uri), CancellationToken.None));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<bool> MkDirAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.MkDir;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Put, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<bool> MTimeAsync(string path, DateTime? newTime = null)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.MTime(newTime);

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Post, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<bool> QuickDeleteAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.QuickDelete;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Post, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<bool> RenameAsync(string path, string destination)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Rename(destination);

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Post, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<bool> RmDirAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.RmDir;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Post, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<string> StatAsync(string path)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Stat();

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Get, Uri), CancellationToken.None));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<bool> SymLinkAsync(string path, string target)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.SymLink(target);

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(HttpMethod.Post, Uri), CancellationToken.None));

      return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadAsync(string path, FileInfo srcFile, bool? indexZip = null)
    {
      if (!srcFile.Exists) throw new FileNotFoundException("Src file is not accessible", srcFile.ToString());

      var mTime = srcFile.LastWriteTime;
      byte[] checksum;
      Stream stream;
      using (stream = new BufferedStream(srcFile.OpenRead(), 1024 * 1024))
      {
        checksum = stream.ComputeHash(HashType.SHA256.Checksum);
      }

      stream = srcFile.OpenRead();
      var size = srcFile.Length;
      return await UploadAsync(path, stream, mTime, size, sha256Checksum: checksum, indexZip: indexZip);
    }

    public async Task<bool> UploadAsync(string path, Stream uploadFileStream, DateTime? mTime = null, long? size = null,
      byte[] md5Checksum = null, byte[] sha1Checksum = null, byte[] sha256Checksum = null, bool? indexZip = null)
    {
      Uri = await GetNetStorageUri(path);
      Params = NetStorageAction.Upload(mTime, size, md5Checksum, sha1Checksum, sha256Checksum, indexZip);

      // sanity check to ensure that indexZip is only true if the file destination is also a zip.
      // probably should throw an exception or warning instead.
      if (Params.IndexZip == true && !path.EndsWith(".zip"))
        Params.IndexZip = null;

      // size is not supported with zip since the index-zip funtionality mutates the file thus inconsistency on which size value to use
      // probably should throw an exception or a warning
      if (Params.Size != null && Params.IndexZip == true)
        Params.Size = null;

      var response = await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() =>
          SendAsync(new HttpRequestMessage(HttpMethod.Put, Uri) {Content = new StreamContent(uploadFileStream)},
            CancellationToken.None));

      return response.IsSuccessStatusCode;
    }
  }
}