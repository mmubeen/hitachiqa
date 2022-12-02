using System;
using System.Collections.Generic;
using System.Linq;

namespace HitachiQA
{
    public class Severity
    {

        public const int
         INT_OFF = 0,
         INT_CRITICAL = 1,
         INT_ERROR = 2,
         INT_WARN = 3,
         INT_INFO = 4,
         INT_DEBUG = 5;

        public static readonly Severity
          DEBUG = new Severity(INT_DEBUG),
          INFO = new Severity(INT_INFO),
          WARN = new Severity(INT_WARN),
          ERROR = new Severity(INT_ERROR),
          CRITICAL = new Severity(INT_CRITICAL),
          DEFAULT = INFO;
       

        public static readonly Dictionary<int, string> SEVERITIES = new Dictionary<int, string>()
        {
            {INT_OFF,     "OFF" },
            {INT_CRITICAL,"CRITICAL"},
            {INT_ERROR,   "ERROR"},
            {INT_WARN,    "WARN"},
            {INT_INFO,    "INFORMATION"},
            {INT_DEBUG,   "DEBUG"},
        };

        public static Severity parseLevel(String level)
        {
            if(String.IsNullOrWhiteSpace(level))
            {
                return DEFAULT;
            }
            try
            {
                if(int.TryParse(level, out int int_level))
                {
                    return new Severity(int_level);
                }
                level = level.ToUpper();
                return new Severity(SEVERITIES.First(it => it.Value == level).Key);
            }
            catch
            {
                return DEFAULT;
            }
        }
        public int Level { get; }
        public string Name { get { return SEVERITIES[Level]; } }
        public Severity(int level)
        {
            if (level>=0 || level<=5)
            {
                this.Level = level;
            }
            else
            {
                throw new ArgumentException($"Severity Level: [{level}] is not valid");
            }
        }
    }
}
