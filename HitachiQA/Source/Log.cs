using HitachiQA.Helpers;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace HitachiQA
{
    public static class Log
    {
        public static void Debug(object text) => Write(Severity.DEBUG, text);
        public static void Info(object text) => Write(Severity.INFO, text);
        public static void Warn(object text) => Write(Severity.WARN, text);
        public static void Error(object text) => Write(Severity.ERROR, text);
        public static void Critical(object text) => Write(Severity.CRITICAL, text);


        public static void Debug(string text, params (string key, dynamic value)[] parameters) => Write(Severity.DEBUG, text, parameters);
        public static void Info(string text, params (string key, dynamic value)[] parameters) => Write(Severity.INFO, text, parameters);
        public static void Warn(string text, params (string key, dynamic value)[] parameters) => Write(Severity.WARN, text, parameters);
        public static void Error(string text, params (string key, dynamic value)[] parameters) => Write(Severity.ERROR, text, parameters);
        public static void Critical(string text, params (string key, dynamic value)[] parameters) => Write(Severity.CRITICAL, text, parameters);

        /// <summary>
        /// Will log text to the the given severity
        /// </summary>
        /// <remarks>
        /// Examples: <br/>
        /// Log.write(Severity.INFO, "informational message") <br/>
        /// OR <br/>
        /// Log.info("informational message") <br/>
        /// </remarks>
        /// 

        public static void Write(Severity severity, object text)
        {
            var currentSev = Severity.parseLevel(Main.Configuration.GetSection("Logging").GetSection("LogLevel")["Default"]).Level;

            if (currentSev == 0)
            {
                return;
            }
            else if (severity.Level <= currentSev)
            {
                var str = stringify(text);
                str = str.Replace($"\n", $"\n[{severity.Name}] ");
                Console.WriteLine($"[{severity.Name}] {str}");
            }
        }

        private static string stringify(object text)
        {
            if (text is Dictionary<string, string> || text is Dictionary<String, String>)
            {
                return stringify(((Dictionary<String, String>)text).Select(entry => $"[{entry.Key}={entry.Value}]"));

            }
            else if (text is IEnumerable<String> || text is IEnumerable<string>)
            {
                var enumStr = (IEnumerable<String>)text;
                if(enumStr.Count()==0)
                {
                    return "";
                }
                var countEnum = enumStr.Select(it => it.Count());
                var sumChar = countEnum.Sum();
                var avgStrSize = countEnum.Average();
                var maxStrSize = countEnum.Max();

                if (avgStrSize > 35)
                {
                    return string.Join(",\n", enumStr);

                }
                var padded = enumStr.Select(it => $"{it}, ".PadRight(maxStrSize));
                return string.Join("", padded);
            }
            else if (text is IEnumerable @enumerable)
            {
                var result = new List<string>();
                foreach (var item in @enumerable)
                {
                    var str = stringify(item);
                    str ??= "NULL";
                    result.Add(str);
                }
                return stringify(result);
            }
            else if (text is string @string)
            {
                return @string;
            }
            else
            {
                return text?.ToString() ?? "NULL";
            }
        }

        public static void Write(Severity severity, string text, params (string key, dynamic value)[] parameters)
        {
            string parsed="";
            foreach(var parameter in parameters)
            {
                parsed = text.Replace(parameter.key, parameter.value);
            }

            Log.Write(severity, (object)parsed);
        
        }


    }
}
