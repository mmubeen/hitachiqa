using HitachiQA.Helpers;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            if(currentSev == 0)
            {
                return;
            }
            else if (severity.Level <= currentSev)
            {
                if (text is Dictionary<string, string>  || text is Dictionary<String, String>)
                {
                    Log.Write(severity, ((Dictionary<String, String>)text).Select(entry => $"{entry.Key}:{entry.Value}"));
                   
                }
                else if(text is IEnumerable<String> || text is IEnumerable<string>)
                {
                    Log.Write(severity, string.Join(", \n", (IEnumerable<String>)text ));
                }
                else if(text is IEnumerable)
                {
                    var result = new List<string>();

                    foreach (var item in (IEnumerable)text)
                    {
                        var str = item?.ToString();
                        str ??= "NULL";
                        result.Add(str);
                    }
                    Log.Write(severity, result);
                }
                else
                {
                    Console.WriteLine($"[{severity.Name}] {text}");
                }
            }
        }

        public static void Write(Severity severity, string text, params (string key, dynamic value)[] parameters)
        {
            foreach(var parameter in parameters)
            {
                if(!parameter.key.StartsWith('@'))
                {
                    throw new Exception($"Parameter key: {parameter.key} should start with @");
                }
                if (parameter.value is IEnumerable)
                {
                    text = text.Replace(parameter.key, EnumerableToHumanReadable((IEnumerable)parameter.value));
                }
                if(parameter.value != null && !(parameter.value is string) && !(parameter.value is String))
                {
                    var paramValue = parameter.value as string;
                    text = text.Replace(parameter.key, paramValue);
                }
                else
                {
                    text = text.Replace(parameter.key, parameter.value);
                }
            }

            var currentSev = Severity.parseLevel(Main.Configuration.GetSection("Logging").GetSection("LogLevel")["Default"]).Level;

            if (currentSev == 0)
            {
                return;
            }
            else if (severity.Level <= currentSev)
            {

                Console.WriteLine($"[{severity.Name}] {text}");
            }
        }

        public static string EnumerableToHumanReadable(IEnumerable list)
        {
            var str = new StringBuilder();
            foreach(var item in list)
            {
                str.Append("\n"+item+", ");
            }
            
            return "\n[" + str.ToString().Trim().Trim(',') + "]\n";
        }
    }
}
