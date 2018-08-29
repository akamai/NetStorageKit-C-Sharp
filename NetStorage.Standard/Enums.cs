// ReSharper disable InconsistentNaming

namespace NetStorage.Standard
{
  /// <summary>
  /// An enum of the hash algorithms supported by <see cref="#ExtensionMethods.ComputeKeyedHash(Stream, KeyedHashAlgorithm"/>
  /// Currently supported hashes include MD5; SHA1; SHA256
  ///
  /// The string representation matches the <see cref="System.Security.Cryptography.HMAC"/> canonical names.
  /// </summary>
  public enum KeyedHashAlgorithm
  {
    HMACSHA256,
    HMACSHA1,
    HMACMD5
  }

  /// <summary>
  /// An enum of the hash algorithms supported by <see cref="#ExtensionMethods.ComputeHash(Stream, ChecksumAlgorithm"/>
  /// Currently supported hashes include MD5; SHA1; SHA256
  ///
  /// The string representation matches the <see cref="System.Security.Cryptography.HashAlgorithm"/> canonical names.
  /// </summary>
  public enum ChecksumAlgorithm
  {
    SHA256,
    SHA1,
    MD5
  }
}