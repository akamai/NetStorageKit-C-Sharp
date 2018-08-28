// ReSharper disable InconsistentNaming

using System;
using System.Runtime.Serialization;

namespace NetStorage.Standard.Models
{
  /// <summary>
  /// The APIParams represents all the possible parameters for the CMS API
  /// 
  /// Verion: always 1
  /// Action: the cms action (eg: "dir", "upload", etc)
  /// Format: for actions that return content, defines the format types (eg: "xml")
  /// QuickDelete: always "imreallyreallysure"
  /// Destination: URI Path for the rename
  /// Target: URI Path of the existing file/dir for a symlink
  /// MTime: modified time
  /// Size: byte size of an uploaded file. NB: do not specify if indexing a zip
  /// MD5: MD5 checksum
  /// SHA1: SHA1 checksum
  /// SHA256: SHA256 checksum
  /// InxedZip: True if the zip file is to be enabled for serve from zip functionality
  /// </summary>
  public class APIParams
  {
    public int Version => 1;

    public string Action { get; set; }
    public string Format { get; set; }

    [DataMember(Name = "quick-delete")] public string QuickDelete { get; set; }
    public string Destination { get; set; }
    public string Target { get; set; }
    public DateTime? MTime { get; set; }
    public long? Size { get; set; }
    public byte[] MD5 { get; set; }
    public byte[] SHA1 { get; set; }
    public byte[] SHA256 { get; set; }
    [DataMember(Name = "index-zip")] public bool? IndexZip { get; set; }
  }
}