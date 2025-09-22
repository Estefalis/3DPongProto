using System;
using System.IO;
using System.Linq;

namespace ThreeDeePongProto.Shared.HelperClasses
{
    internal static class StringManipulation
    {
        /// <summary>
        /// Returns a string with character(s) between the submitted arguments, if at least one is found. Else returns "".
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_firstArg"></param>
        /// <param name="_secondArg"></param>
        /// <returns></returns>
        internal static string GetCharacterBetweenArgs(string _source, string _firstArg, string _secondArg)
        {
            if (string.IsNullOrEmpty(_source) || !_source.Contains(_firstArg) || !_source.Contains(_secondArg))
            {
                return string.Empty;
            }

            int start = _source.IndexOf(_firstArg, StringComparison.Ordinal) + _firstArg.Length;
            int end = _source.IndexOf(_secondArg, start, StringComparison.Ordinal);
            return (end > start) ? _source[start..end].Trim() : string.Empty;
        }

        /// <summary>
        /// Returns result as Int, if TryParse is true. Else returns int.MinValue.
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_firstArg"></param>
        /// <param name="_secondArg"></param>
        /// <returns></returns>
        internal static int GetIntBetweenArgs(string _source, string _firstArg, string _secondArg)
        {
            if (_source.Contains(_firstArg) && _source.Contains(_secondArg))
            {
                int start = _source.IndexOf(_firstArg, 0) + _firstArg.Length;
                int end = _source.IndexOf(_secondArg, start);
                //int returnValue = int.Parse(_source.Substring(start, end - start));
                //bool resultIsInt = int.TryParse(_source.Substring(start, end - start), out int result);
                bool resultIsInt = int.TryParse(_source[start..end], out int result);

                if (resultIsInt)
                    return result;
            }

            return int.MinValue;
        }

        internal static bool ContainsPlayerID(string _source, int _playerID, int _digitCount, SearchDirection _direction)
        {
            string idString = _playerID.ToString().PadLeft(_digitCount, '0');

            return _direction switch
            {
                SearchDirection.Forwards => _source.StartsWith(idString),
                SearchDirection.Backwards => _source.EndsWith(idString),
                SearchDirection.FullScan => _source.Contains(idString),
                _ => false,
            };
        }

        /// <summary>
        /// Returns a new string with a big character at the submitted index. An index of 0, or none submitted, equals first letter.
        /// </summary>
        /// <param name="_source"></param>
        /// <returns></returns>
        internal static string ToUpperCharacterAtIndex(string _source, int _charIndex = 0)
        {
            if (string.IsNullOrEmpty(_source))
                return _source;

            char[] letters = _source.ToCharArray();
            letters[0] = char.ToUpper(letters[_charIndex]);
            return new string(letters);
        }

        internal static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("Paths are null.");
            }
            return paths.Aggregate(Path.Combine);
        }
    }
}