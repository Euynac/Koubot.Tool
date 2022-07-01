﻿using JetBrains.Annotations;
using Koubot.Tool.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Koubot.Tool.General
{
    /// <summary>
    /// For easily debug
    /// </summary>
    public static class DebugTool
    {
        /// <summary>
        /// Use <see cref="System.Text.Json.JsonSerializer"/> to serialize given object.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="writeIndented">A value that defines whether JSON should use pretty printing.
        /// <see langword="true" /> if JSON should pretty print on serialization; otherwise, <see langword="false" />. The default is <see langword="false" />.
        /// </param>
        /// <remarks>Not support serialize <see cref="Exception"/> and <see cref="Type"/>. See <see href="https://github.com/dotnet/runtime/issues/43026"/> and <see href="https://github.com/dotnet/runtime/issues/31567#issuecomment-558335944"/></remarks>
        /// <returns></returns>
        public static string? ToJsonString(this object? s, bool writeIndented = true)
        {
            if (s == null) return null;
            var options = new JsonSerializerOptions
            {
                WriteIndented = writeIndented,
            };
            if (writeIndented)
            {
                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            }

            var json = JsonSerializer.Serialize(s, options);
            return writeIndented ? Regex.Unescape(json) : json;
        }

        /// <summary>
        /// Force <see cref="System.Text.Json.JsonSerializer"/> to serialize given object when normally encounter exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="writeIndented"></param>
        /// <remarks><see cref="System.Text.Json.JsonSerializer"/> is not support to serialize <see cref="Exception"/> and <see cref="Type"/>. See <see href="https://github.com/dotnet/runtime/issues/43026"/> and <see href="https://github.com/dotnet/runtime/issues/31567#issuecomment-558335944"/></remarks>
        /// <returns>Not support to deserialize, only use to print Exception or other type info.</returns>
        public static string? ToJsonStringForce<T>(this T? s, bool writeIndented = true)
        {
            if (s == null) return null;
            var options = new JsonSerializerOptions
            {
                WriteIndented = writeIndented,
            };
            if (writeIndented)
            {
                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            }

            options.Converters.Add(new AnyConverter<T>());
            var json = JsonSerializer.Serialize(s, options);
            return writeIndented ? Regex.Unescape(json) : json;
        }
        

        /// <summary>
        /// ToString() and Console.WriteLine() this obj.
        /// </summary>
        /// <param name="s"></param>
        public static void PrintLn(this object? s) => Console.WriteLine(s?.ToString());
        /// <summary>
        /// ToString() and Console.WriteLine() this obj.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="format">Description of given obj to print.</param>
        public static void PrintLn(this object? s, string format) => Console.WriteLine(format + s);
        /// <summary>
        /// string.Format() and Console.WriteLine() this obj.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="format">format string to print</param>
        /// <param name="useFormat">Placeholder, either true or false will still use string.Format</param>
        [StringFormatMethod("format")]
        public static void PrintLn(this object? s, string format, bool useFormat) => Console.WriteLine(format, s);
        /// <summary>
        /// Use string.join() and ToString() to format like an array, and finally Console.WriteLine() this obj.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        public static void PrintLn<T>(this ICollection<T> objList) => Console.WriteLine($"[{objList.ToStringJoin(",")}]");
        /// <summary>
        /// Format dictionary into Console.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        public static void PrintLn<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            foreach (var pair in dictionary)
            {
                Console.WriteLine($"{pair.Key} —— {pair.Value}");
            }
        }
    }

    /// <summary>
    /// Any property not supported to serialize will simply call ToString() instead.
    /// </summary>
    /// <typeparam name="TAnyType"></typeparam>
    public class AnyConverter<TAnyType> : JsonConverter<TAnyType>
    {
        public override TAnyType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserializing exceptions is not allowed");
        }

        public override void Write(Utf8JsonWriter writer, TAnyType value, JsonSerializerOptions options)
        {
            var properties = value!.GetType()
                .GetProperties()
                .Select(p =>
                {
                    object valueObj;
                    try
                    {
                        valueObj = p.GetValue(value);
                    }
                    catch (Exception)
                    {
                        valueObj = value.ToString();
                    }

                    return new {p.Name, Value = valueObj};
                });
            
            if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                properties = properties.Where(p => p.Value != null);
            }

            var propList = properties.ToList();

            if (propList.Count == 0)
            {
                // Nothing to write
                return;
            }

            writer.WriteStartObject();
            var newOptions = new JsonSerializerOptions().CloneParameters(options, nameof(options.Converters));
            foreach (var prop in propList)
            {
                if (prop.Value == null)
                {
                    writer.WriteString(prop.Name, "null");
                    continue;
                }
                string serializedPropValue;

                try
                {
                    serializedPropValue = JsonSerializer.Serialize(prop.Value, newOptions);
                }
                catch (Exception)
                {
                    serializedPropValue = prop.Value.ToString();
                }
                writer.WriteString(prop.Name, serializedPropValue);
            }

            writer.WriteEndObject();
        }
    }
    /// <summary>
    /// Update the display without flicker
    /// </summary>
    public class ConsoleWriteUpdater
    {
        private (int, int)? _previousCursorPosition;

        /// <summary>
        /// Update use given string builder. (Will make the cursor fixed where the first time you use the update method)
        /// The effect depends on your console window size.
        /// </summary>
        /// <param name="stringBuilder"></param>
        public void Update(StringBuilder stringBuilder) => Update(stringBuilder.ToString());
        /// <summary>
        /// Update use given string. (Will make the cursor fixed where the first time you use the update method)
        /// The effect depends on your console window size.
        /// </summary>
        /// <param name="data"></param>
        public void Update(string data)
        {
            if (_previousCursorPosition == null)
            {
                _previousCursorPosition = (Console.CursorLeft, Console.CursorTop);//The cursor position depends on your console window size.
            }
            else
            {
                Console.SetCursorPosition(_previousCursorPosition.Value.Item1, _previousCursorPosition.Value.Item2);
            }
            Console.Write(data);
        }

        // can't support \n
        // private int _previousDataLength = 0;
        // public void Update(string data)
        // {
        //     data ??= "";
        //     var backup = new string('\b', _previousDataLength);
        //     Console.Write(backup);
        //     Console.Write(data);
        //     _previousDataLength = data.Length;
        // }
    }
}