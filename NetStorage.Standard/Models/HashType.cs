// ReSharper disable InconsistentNaming

namespace NetStorage.Standard.Models
{
  public class HashType
  {
    public static HashType MD5 = new HashType(ChecksumAlgorithm.MD5);
    public static HashType SHA1 = new HashType(ChecksumAlgorithm.SHA1);
    public static HashType SHA256 = new HashType(ChecksumAlgorithm.SHA256);

    public ChecksumAlgorithm Checksum { get; }

    private HashType(ChecksumAlgorithm checksum)
    {
      Checksum = checksum;
    }
  }
}