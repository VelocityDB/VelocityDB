namespace VelocityDBExtensions.CompressedBitArray
{
  /*
   * Copyright 2012, Kemal Erdogan, Daniel Lemire and Ciaran Jessup
   * Licensed under APL 2.0.
   */


  /// <summary>
  /// The class EwahEnumerator represents a special type of
  /// efficient enumerator iterating over (uncompressed) words of bits.
  /// </summary>
  public sealed class EwahEnumerator
  {

    /// <summary>
    /// current running length word
    /// </summary>
    public readonly RunningLengthWord _Rlw;

    /// <summary>
    /// The size in words
    /// </summary>
    private readonly int _SizeInWords;

    /// <summary>
    /// The pointer represent the location of the current running length
    /// word in the array of words (embedded in the rlw attribute).
    /// </summary>
    private int _Pointer;

    /// <summary>
    /// Instantiates a new eWAH enumerator
    /// </summary>
    /// <param name="a">the array of words</param>
    /// <param name="sizeinwords">the number of words that are significant in the array of words</param>
    public EwahEnumerator(long[] a, int sizeinwords)
    {
      _Rlw = new RunningLengthWord(a, 0);
      _SizeInWords = sizeinwords;
      _Pointer = 0;
    }

    /// <summary>
    ///  Access to the array of words
    /// </summary>
    public long[] Buffer
    {
      get { return _Rlw.ArrayOfWords; }
    }

    /// <summary>
    /// Position of the dirty words represented by this running length word
    /// </summary>
    public int DirtyWords
    {
      get { return _Pointer - (int)_Rlw.NumberOfLiteralWords; }
    }

    /// <summary>
    /// Checks for next
    /// </summary>
    /// <returns>true, if successful</returns>
    public bool HasNext()
    {
      return _Pointer < _SizeInWords;
    }

    /// <summary>
    /// Next running length word
    /// </summary>
    /// <returns></returns>
    public RunningLengthWord Next()
    {
      _Rlw.Position = _Pointer;
      _Pointer += (int)_Rlw.NumberOfLiteralWords + 1;
      return _Rlw;
    }

    /// <summary>
    /// Reset the enumerator to the beginning
    /// </summary>
    internal void Reset()
    {
      _Rlw.Position = 0;
      _Pointer = 0;
    }
  }
}