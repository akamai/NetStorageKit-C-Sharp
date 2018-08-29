// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<HttpResponseMessage> ExecuteWithPollyAsync(string path, HttpMethod method)
    {
      Uri = await GetNetStorageUri(path);

      return await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2))
        .ExecuteAsync(() => SendAsync(new HttpRequestMessage(method, Uri), CancellationToken.None));
    }

    public new async Task<HttpResponseMessage> DeleteAsync(string path)
    {
      Params = NetStorageAction.Delete;
      return await ExecuteWithPollyAsync(path, HttpMethod.Put);
    }

    public async Task<HttpResponseMessage> DirAsync(string path)
    {
      Params = NetStorageAction.Dir();
      return await ExecuteWithPollyAsync(path, HttpMethod.Get);
    }

    public async Task<HttpResponseMessage> DownloadAsync(string path)
    {
      Params = NetStorageAction.Download;
      return await ExecuteWithPollyAsync(path, HttpMethod.Get);
    }

    public async Task<HttpResponseMessage> DUAsync(string path)
    {
      Params = NetStorageAction.DU();
      return await ExecuteWithPollyAsync(path, HttpMethod.Get);
    }

    public async Task<HttpResponseMessage> ListAsync(string path)
    {
      Params = NetStorageAction.List();
      return await ExecuteWithPollyAsync(path, HttpMethod.Get);
    }

    public async Task<HttpResponseMessage> MkDirAsync(string path)
    {
      Params = NetStorageAction.MkDir;
      return await ExecuteWithPollyAsync(path, HttpMethod.Put);
    }

    public async Task<HttpResponseMessage> MTimeAsync(string path, DateTime? newTime = null)
    {
      Params = NetStorageAction.MTime(newTime);
      return await ExecuteWithPollyAsync(path, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> QuickDeleteAsync(string path)
    {
      Params = NetStorageAction.QuickDelete;
      return await ExecuteWithPollyAsync(path, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> RenameAsync(string path, string destination)
    {
      Params = NetStorageAction.Rename(destination);
      return await ExecuteWithPollyAsync(path, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> RmDirAsync(string path)
    {
      Params = NetStorageAction.RmDir;
      return await ExecuteWithPollyAsync(path, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> StatAsync(string path)
    {
      Params = NetStorageAction.Stat();
      return await ExecuteWithPollyAsync(path, HttpMethod.Get);
    }

    public async Task<HttpResponseMessage> SymLinkAsync(string path, string target)
    {
      Params = NetStorageAction.SymLink(target);
      return await ExecuteWithPollyAsync(path, HttpMethod.Post);
    }

    public async Task<HttpResponseMessage> UploadAsync(string path, FileInfo srcFile, bool? indexZip = null)
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

    public async Task<HttpResponseMessage> UploadAsync(string path, Stream uploadFileStream, DateTime? mTime = null,
      long? size = null,
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

      return await Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode == false)
        .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5))
        .ExecuteAsync(() =>
          SendAsync(new HttpRequestMessage(HttpMethod.Put, Uri) {Content = new StreamContent(uploadFileStream)},
            CancellationToken.None));
    }
  }
}