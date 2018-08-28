// ReSharper disable InconsistentNaming

namespace NetStorage.Standard.Models
{
  public class NetStorageCredentials
  {
    public string HostName { get; }
    public string Username { get; }
    public string Key { get; }
    public bool UseSSL { get; }

    public NetStorageCredentials(string hostName, string username, string key, bool useSSL = false)
    {
      HostName = hostName;
      Username = username;
      Key = key;
      UseSSL = useSSL;
    }
  }
}