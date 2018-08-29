// ReSharper disable InconsistentNaming

using System;

namespace NetStorage.Standard.Models
{
  public static class NetStorageAction
  {
    public static APIParams Delete = new APIParams {Action = "delete"};
    public static APIParams Dir(string format = "xml") => new APIParams {Action = "dir", Format = format};
    public static APIParams Download = new APIParams {Action = "download"};
    public static APIParams DU(string format = "xml") => new APIParams {Action = "du", Format = format};
    public static APIParams List(string format = "xml") => new APIParams {Action = "list", Format = format};
    public static APIParams MkDir = new APIParams {Action = "mkdir"};

    public static APIParams MTime(DateTime? mTime = null) =>
      new APIParams {Action = "mtime", MTime = mTime ?? DateTime.Now};

    public static APIParams QuickDelete = new APIParams {Action = "quick-delete", QuickDelete = "imreallyreallysure"};
    public static APIParams Rename(string destination) => new APIParams {Action = "rename", Destination = destination};
    public static APIParams RmDir = new APIParams {Action = "rmdir"};
    public static APIParams Stat(string format = "xml") => new APIParams {Action = "stat", Format = format};
    public static APIParams SymLink(string target) => new APIParams {Action = "symlink", Target = target};

    public static APIParams Upload(DateTime? mTime = null, long? size = null, byte[] md5Checksum = null,
      byte[] sha1Checksum = null, byte[] sha256Checksum = null, bool? indexZip = null) =>
      new APIParams
      {
        Action = "upload",
        MTime = mTime,
        Size = size,
        MD5 = md5Checksum,
        SHA1 = sha1Checksum,
        SHA256 = sha256Checksum,
        IndexZip = indexZip == true ? indexZip : null
      };
  }
}