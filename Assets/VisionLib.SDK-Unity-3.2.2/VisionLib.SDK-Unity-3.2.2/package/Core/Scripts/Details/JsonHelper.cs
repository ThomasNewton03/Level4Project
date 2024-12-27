using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    ///  VisionLib functions for working with JSON data.
    /// </summary>
    /// <remarks>
    ///  Right now it's just using the JsonUtility class from UnityEngine.
    /// </remarks>
    public static class JsonHelper
    {
        public interface IJsonParsable
        {
            bool IsValid();
            string GetJsonName();
            string GetWarning();
        }

        public class AttributeNotFoundException : Exception
        {
            public AttributeNotFoundException(string message)
                : base(message) {}
        }

        public static string ConditionJson(string json)
        {
            var result = json.Trim();
            if (result[0] != '{')
            {
                result = "{" + result + "}";
            }
            return result;
        }

        public static T? FromNullableJson<T>(string json) where T : struct
        {
            if (json == null)
            {
                return null;
            }
            return FromJson<T>(json);
        }

        public static T FromJson<T>(string json)
        {
            return UnityEngine.JsonUtility.FromJson<T>(json);
        }

        public static void FromJsonOverwrite(string json, object objectToOverwrite)
        {
            UnityEngine.JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
        }

        public static string ToJson(object obj)
        {
            return UnityEngine.JsonUtility.ToJson(obj);
        }

        public static string ToJson(object obj, bool prettyPrint)
        {
            return UnityEngine.JsonUtility.ToJson(obj, prettyPrint);
        }

        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            NextIndentedLine(sb, ++indent);
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            NextIndentedLine(sb, --indent);
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                        {
                            escaped = !escaped;
                        }

                        if (!escaped)
                        {
                            quoted = !quoted;
                        }
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            NextIndentedLine(sb, indent);
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted && i + 1 < str.Length && str[i + 1] != ' ')
                        {
                            sb.Append(" ");
                        }
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void NextIndentedLine(StringBuilder sb, int indentationLevel)
        {
            const string indentString = "    ";

            sb.AppendLine();
            for (var i = 0; i < indentationLevel; i++)
            {
                sb.Append(indentString);
            }
        }

        internal static string ValueToJsonString<T>(T value) where T : struct
        {
            if (typeof(T) == typeof(Color))
            {
                var colorValue = (Color) (object) value;
                return "[" + (int) (255 * colorValue.r) + ", " + (int) (255 * colorValue.g) + ", " +
                       (int) (255 * colorValue.b) + "]";
            }
            if (typeof(T) == typeof(bool))
            {
                var boolValue = (bool) (object) value;
                return boolValue.ToInvariantLowerCaseString();
            }
            if (typeof(T) == typeof(int))
            {
                return value.ToString();
            }
            throw new ArgumentException(
                "Only the types 'Color', 'bool' and 'int' are currently supported.");
        }

        internal static T InterpretAs<T>(JToken value)
        {
            var targetType = typeof(T);
            if (targetType == typeof(Color))
            {
                return (T) (object) ((JArray) value).ToColor();
            }
            if (targetType == typeof(bool) || targetType == typeof(int) ||
                targetType == typeof(float) || targetType == typeof(string))
            {
                return value.ToObject<T>();
            }
            if (typeof(T) == typeof(ShowLineModel))
            {
                return (T) (object) value.ToShowLineModel();
            }
            throw new ArgumentException(
                "Only the types 'Color', 'bool', 'float', 'int', 'string' " +
                "and 'ShowLineModel' are currently supported.");
        }

        internal static T ParseJsonValueFromBackendAs<T>(string parameterJsonString)
        {
            JToken jToken;
            try
            {
                jToken = JToken.Parse(parameterJsonString);
            }
            catch (JsonReaderException)
            {
                jToken = JToken.Parse($"\"{parameterJsonString}\"");
            }
            var targetType = typeof(T);
            if (targetType == typeof(string) || targetType == typeof(bool) ||
                targetType == typeof(float) || targetType == typeof(int))
            {
                if (jToken.GetType() != typeof(JValue))
                {
                    throw new ArgumentException(
                        $"The provided JSON is not a primitive of type '{targetType}'. " +
                        $"JSON String: {parameterJsonString}.");
                }
            }
            else if (targetType != typeof(ShowLineModel))
            {
                throw new ArgumentException(
                    "Only the types 'string', 'bool' and 'float' and 'ShowLineModel' " +
                    "are currently supported.");
            }
            return InterpretAs<T>(jToken);
        }

        internal static ShowLineModel ToShowLineModel(this JToken jToken)
        {
            if (jToken.GetType() == typeof(JValue))
            {
                return new ShowLineModel(InterpretAs<bool>(jToken));
            }

            if (jToken.GetType() == typeof(JObject))
            {
                return ((JObject) jToken).ToShowLineModel();
            }

            throw new ArgumentException(
                $"Unexpected parameter JSON token. Token String: {jToken}.");
        }

        internal static ShowLineModel ToShowLineModel(this JObject jObject)
        {
            var enabled = new LineModelParameter<bool>(ShowLineModel.defaultEnabledValue);
            var enabledJson = jObject.GetValue(ShowLineModel.enabledJsonAttributeName);
            if (enabledJson != null)
            {
                enabled = enabledJson.ToLineModelParameter<bool>();
            }

            var drawLineModels = enabled.GetSharedValueForAllStates()
                .GetValueOrDefault(ShowLineModel.defaultEnabledValue);
            
            var color = new LineModelColor();
            var colorJson = jObject.GetValue(ShowLineModel.colorJsonAttributeName);
            if (colorJson != null)
            {
                color = colorJson.ToLineModelParameterColor();
            }

            var lineWidth = new LineModelParameter<int>(ShowLineModel.defaultLineWidth);
            var lineWidthJson = jObject.GetValue(ShowLineModel.lineWidthJsonAttributeName);
            if (lineWidthJson != null)
            {
                lineWidth = lineWidthJson.ToLineModelParameter<int>();
            }

            return new ShowLineModel
            {
                enabled = enabled,
                color = color,
                lineWidth = lineWidth
            };
        }

        private static LineModelColor ToLineModelParameterColor(this JToken jToken)
        {
            if (jToken.GetType() == typeof(JValue) && string.Equals(
                    jToken.ToString(),
                    "perCorrespondency",
                    StringComparison.Ordinal))
            {
                return new LineModelColor {perCorrespondency = true};
            }
            return new LineModelColor(jToken.ToLineModelParameter<Color>());
        }

        private static LineModelParameter<T> ToLineModelParameter<T>(this JToken jToken)
            where T : struct
        {
            if (jToken.GetType() == typeof(JValue) || jToken.GetType() == typeof(JArray))
            {
                var tokenValue = InterpretAs<T>(jToken);
                return new LineModelParameter<T>
                {
                    tracked = tokenValue, critical = tokenValue, lost = tokenValue
                };
            }
            if (jToken.GetType() == typeof(JObject))
            {
                var parameter = new LineModelParameter<T>();
                var parametersJson = (JObject) jToken;
                parameter.tracked = InterpretAs<T>(
                    parametersJson.GetTokenOrThrow(LineModelParameter<T>.trackedJsonAttributeName));
                parameter.critical = InterpretAs<T>(
                    parametersJson.GetTokenOrThrow(
                        LineModelParameter<T>.criticalJsonAttributeName));
                parameter.lost = InterpretAs<T>(
                    parametersJson.GetTokenOrThrow(LineModelParameter<T>.lostJsonAttributeName));
                return parameter;
            }
            throw new ArgumentException("\"jToken\" must be either a value or a JSON object.");
        }

        private static JToken GetTokenOrThrow(this JObject jObject, string key)
        {
            return jObject.GetValue(key) ??
                   throw new AttributeNotFoundException($"JSON is missing the {key} attribute.");
        }

        private static Color ToColor(this JArray jColorValues)
        {
            var colorValues = jColorValues.Select(
                token => token is not JValue ? null : token.Value<int?>() / 255f).ToArray();

            var numColorValues = jColorValues.Count;
            if (new[] {3, 4}.All(numElements => numColorValues != numElements) ||
                colorValues.Any(value => !value.HasValue))
            {
                throw new ArgumentException(
                    $"Provided JArray '{jColorValues.ToString(Formatting.None)}' does not have the expected JSON color format.");
            }

            return numColorValues switch
            {
                3 => new Color(colorValues[0].Value, colorValues[1].Value, colorValues[2].Value),
                4 => new Color(
                    colorValues[0].Value,
                    colorValues[1].Value,
                    colorValues[2].Value,
                    colorValues[3].Value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
