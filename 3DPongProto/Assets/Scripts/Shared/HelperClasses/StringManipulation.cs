namespace ThreeDeePongProto.Shared.HelperClasses
{
    internal static class StringManipulation
    {
        internal static string GetWordBetweenArgs(string _source, string _firstArg, string _secondArg)
        {
            if (_source.Contains(_firstArg) && _source.Contains(_secondArg))
            {
                int start = _source.IndexOf(_firstArg, 0) + _firstArg.Length;
                int end = _source.IndexOf(_secondArg, start);
                return _source.Substring(start, end - start);
            }

            return "";
        }

        internal static string ToUpperFirstCharacter(string _source)
        {
            if (string.IsNullOrEmpty(_source))
                return _source;

            char[] letters = _source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        /// <summary>
        /// Returns int.MinValue, once the submitted digit amount hits the first none-int letter on TryParse. 0 = 1 digit.
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_digitsLikeIndexCount"></param>
        /// <returns></returns>
        internal static int GetIndexAtStringEnd(string _source, int _digitsLikeIndexCount = 0)
        {
            string parseSlots = _source.Substring(_source.Length - (1 + _digitsLikeIndexCount));
            bool returnSlots = int.TryParse(parseSlots, out int result);

            if (returnSlots)
                return result;
            else
                return int.MinValue;
        }
    }
}