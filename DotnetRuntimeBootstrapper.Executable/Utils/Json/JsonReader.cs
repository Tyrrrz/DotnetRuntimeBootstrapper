using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonReader
    {
        private readonly string _source;
        private int _index;

        public JsonReader(string source) => _source = source;

        private T? WithBacktracking<T>(Func<T?> tryParse)
        {
            var indexCheckpoint = _index;
            var result = tryParse();

            // Restore checkpoint if parsing failed
            if (result is null)
                _index = indexCheckpoint;

            return result;
        }

        private char? TryPeek() =>
            _index < _source.Length
                ? _source[_index]
                : null;

        private char? TryRead()
        {
            var c = TryPeek();

            if (c is not null)
                _index++;

            return c;
        }

        private bool TryPeek(char expectedChar) =>
            TryPeek() == expectedChar;

        private bool TryRead(char expectedChar)
        {
            if (TryPeek(expectedChar))
            {
                _index++;
                return true;
            }

            return false;
        }

        private string? TryPeek(int length) =>
            _index + length <= _source.Length
                ? _source.Substring(_index, length)
                : null;

        private bool TryPeek(string expectedString) => string.Equals(
            TryPeek(expectedString.Length),
            expectedString,
            StringComparison.Ordinal
        );

        private bool TryRead(string expectedString)
        {
            if (TryPeek(expectedString))
            {
                _index += expectedString.Length;
                return true;
            }

            return false;
        }

        private void SkipWhiteSpace()
        {
            while (true)
            {
                var c = TryPeek();
                if (c is null || !char.IsWhiteSpace(c.Value))
                    break;

                _index++;
            }
        }

        private JsonNode? TryReadNull() => WithBacktracking(() =>
            TryRead("null")
                ? JsonNull.Instance
                : null
        );

        private JsonNode? TryReadBool()
        {
            JsonNode? TryReadBoolTrue() => WithBacktracking(() =>
                TryRead("true")
                    ? JsonBool.True
                    : null
            );

            JsonNode? TryReadBoolFalse() => WithBacktracking(() =>
                TryRead("false")
                    ? JsonBool.False
                    : null
            );

            return TryReadBoolTrue() ?? TryReadBoolFalse();
        }

        private JsonNode? TryReadNumber() => WithBacktracking(() =>
        {
            var buffer = new StringBuilder();
            while (true)
            {
                var peek = TryPeek();

                // Only digit characters and floating point separator character
                if (peek is null || !(char.IsDigit(peek.Value) || peek == '.'))
                    break;

                var c = TryRead();
                if (c is null)
                    break;

                buffer.Append(c.Value);
            }

            return double.TryParse(buffer.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                ? new JsonNumber(value)
                : null;
        });

        private JsonNode? TryReadString() => WithBacktracking(() =>
        {
            if (!TryRead('"'))
                return null;

            var buffer = new StringBuilder();
            while (true)
            {
                var prev = buffer.Length > 0
                    ? buffer[buffer.Length - 1]
                    : (char?) null;

                // Read until (unescaped) closing double quote
                if (TryPeek('"') && prev != '\\')
                    break;

                var c = TryRead();
                if (c is null)
                    break;

                buffer.Append(c.Value);
            }

            if (!TryRead('"'))
                return null;

            return new JsonString(buffer.ToString());
        });

        private JsonNode? TryReadArray() => WithBacktracking(() =>
        {
            if (!TryRead('['))
                return null;

            var children = new List<JsonNode>();
            while (true)
            {
                SkipWhiteSpace();

                var child = TryReadNode();
                if (child is null)
                    break;

                children.Add(child);

                SkipWhiteSpace();

                // Items are separated by commas
                if (!TryRead(','))
                    break;
            }

            if (!TryRead(']'))
                return null;

            return new JsonArray(children.ToArray());
        });

        private JsonProperty? TryReadProperty() => WithBacktracking(() =>
        {
            var name = TryReadString()?.TryGetString();
            if (name is null)
                return null;

            SkipWhiteSpace();

            if (!TryRead(':'))
                return null;

            SkipWhiteSpace();

            var value = TryReadNode();
            if (value is null)
                return null;

            return new JsonProperty(name, value);
        });

        private JsonNode? TryReadObject() => WithBacktracking(() =>
        {
            if (!TryRead('{'))
                return null;

            var properties = new List<JsonProperty>();
            while (true)
            {
                SkipWhiteSpace();

                var property = TryReadProperty();
                if (property is null)
                    break;

                properties.Add(property);

                SkipWhiteSpace();

                if (!TryRead(','))
                    break;
            }

            if (!TryRead('}'))
                return null;

            return new JsonObject(properties.ToArray());
        });

        private JsonNode? TryReadNode() =>
            TryReadNull() ??
            TryReadBool() ??
            TryReadNumber() ??
            TryReadString() ??
            TryReadArray() ??
            TryReadObject();

        public JsonNode? TryReadDocument()
        {
            var node = TryReadNode();
            if (node is null)
                return null;

            // Ensure that the entire input has been consumed (disregarding whitespace)
            SkipWhiteSpace();
            if (_index < _source.Length)
                return null;

            return node;
        }
    }
}