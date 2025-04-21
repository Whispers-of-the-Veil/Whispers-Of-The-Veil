public static class StringExtensions {
    /// <summary>
    /// Extension method to the string type to compute the FNV-1a has for the input string.
    /// The FNV-1a has is a non-cryptographic has function known for its speed and good distribution properties.
    /// Useful for creating Dictionary keys instead of using strings.
    ///
    /// This method was written by git-amend
    /// </summary>
    /// <param name="str">The inptu string to hash</param>
    /// <returns>An integer representing the FNV-1a hash of th einput string.</returns>
    public static int ComputeFNV1aHash(this string str) {
        uint hash = 2166136261;
        foreach (char c in str) {
            hash = (hash ^ c) * 16777619;
        }
        return unchecked((int)hash);
    }
}