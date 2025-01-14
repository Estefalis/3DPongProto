using System.IO;
using System.Linq;
using System;

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
            if (_source.Contains(_firstArg) && _source.Contains(_secondArg))
            {
                int start = _source.IndexOf(_firstArg, 0) + _firstArg.Length;
                int end = _source.IndexOf(_secondArg, start);
                return _source.Substring(start, end - start);
            }

            return "";
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

        #region Replaced
        #region GetIndexAtStringEnd
        ///// <summary>
        ///// Returns int.MinValue, once the submitted digit amount hits the first none-int letter on TryParse. 0 = 1 digit.
        ///// </summary>
        ///// <param name="_source"></param>
        ///// <param name="_digitsLikeIndexCount"></param>
        ///// <returns></returns>
        //internal static int GetIndexAtStringEnd(string _source, int _digitsLikeIndexCount = 0)
        //{
        //    string parseSlots = _source.Substring(_source.Length - (1 + _digitsLikeIndexCount));
        //    bool returnSlots = int.TryParse(parseSlots, out int result);

        //    if (returnSlots)
        //        return result;
        //    else
        //        return int.MinValue;
        //} 
        #endregion
        #endregion
    }
}