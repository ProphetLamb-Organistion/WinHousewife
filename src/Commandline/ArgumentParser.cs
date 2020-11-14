using System;
using System.Collections;
using System.Collections.Generic;

namespace IntegratedCalc.Commandline
{

    public enum CommandLineArgumentParserOptions
    {
        None = 0,
        RemoveEmptyEntries = 1,
        RemoveWhitespaceEntries = 3,
        RemoveEncompassingQuotes = 4,
        UnescapeCharacters = 8
    }

    public class CommandLineArgumentParser : IEnumerator<string>
    {
        readonly string _input;
        CommandLineArgumentParserOptions _options;
        string _token = String.Empty;
        int _currentTokenPos;

        public CommandLineArgumentParser(string input, CommandLineArgumentParserOptions options = CommandLineArgumentParserOptions.None)
        {
            _input = input;
            _options = options;
        }

        public string Current => _token;
        object IEnumerator.Current => _token;

        public bool MoveNext()
        {
            if (_currentTokenPos >= _input.Length)
            {
                _token = String.Empty;
                return false;
            }
            ReadOnlySpan<char> token = _input.AsSpan().Slice(_currentTokenPos);
            bool escapeNextChar = false;
            bool quoteBlock = false;
            for (int i = 0; i < token.Length; i++)
            {
                switch (token[i])
                {
                    case ' ':
                        if (escapeNextChar)
                            escapeNextChar = false;
                        if (!quoteBlock)
                        {
                            return SetNext(token.Slice(0, i), i + 1);
                        }
                        break;
                    case '\"':
                        if (escapeNextChar)
                        {
                            escapeNextChar = false;
                            break;
                        }
                        if (quoteBlock)
                        {
                            i++;
                            return SetNext(token.Slice(0, i), i + 1);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                quoteBlock = true;
                            }
                            else
                            {
                                i--;
                                // end
                                _token = token.Slice(0, i).ToString();
                                _currentTokenPos += i + 1;
                                return true;
                            }
                        }
                        break;
                    case '\\':
                        if (escapeNextChar)
                            escapeNextChar = false;
                        else
                            escapeNextChar = true;
                        break;
                }
            }
            return SetNext(token, token.Length + 1);
        }

        private bool SetNext(ReadOnlySpan<char> nextCanidate, int consumedChars)
        {
            if ((_options & CommandLineArgumentParserOptions.RemoveEncompassingQuotes) != 0 && nextCanidate[0] == '\"' && nextCanidate[nextCanidate.Length - 1] == '\"')
                nextCanidate = nextCanidate.Slice(1, nextCanidate.Length - 3);
            if ((_options & CommandLineArgumentParserOptions.RemoveWhitespaceEntries) != 0 && nextCanidate.IsWhiteSpace())
                return MoveNext();
            if ((_options & CommandLineArgumentParserOptions.RemoveEmptyEntries) != 0 && nextCanidate.IsEmpty)
                return MoveNext();
            if ((_options & CommandLineArgumentParserOptions.UnescapeCharacters) != 0)
                nextCanidate = System.Text.RegularExpressions.Regex.Unescape(nextCanidate.ToString()).AsSpan();

            _token = nextCanidate.ToString();
            _currentTokenPos += consumedChars;
            return true;
        }

        public ReadOnlySpan<char> Remaining()
        {
            return _input.AsSpan().Slice(_currentTokenPos);
        }

        public void Reset()
        {
            _currentTokenPos = 0;
            _token = String.Empty;
        }

        #region IDisposable members
        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
