// ReSharper disable InconsistentNaming

namespace NetStorage.Standard.Models
{
  public class SignType
  {
    public static SignType HMACMD5 = new SignType(3, KeyedHashAlgorithm.HMACMD5);
    public static SignType HMACSHA1 = new SignType(4, KeyedHashAlgorithm.HMACSHA1);
    public static SignType HMACSHA256 = new SignType(5, KeyedHashAlgorithm.HMACSHA256);

    public int VersionID { get; }
    public KeyedHashAlgorithm Algorithm { get; }

    private SignType(int versionID, KeyedHashAlgorithm algorithm)
    {
      VersionID = versionID;
      Algorithm = algorithm;
    }
  }
}