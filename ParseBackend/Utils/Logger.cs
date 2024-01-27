using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace ParseBackend.Utils
{
    public static class Logger
    {
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            Console.Write($"[{DateTime.Now.ToString("H:m:s")}]");
            Console.Write("[");

            Console.ForegroundColor = level switch
            {
                LogLevel.Info => ConsoleColor.Cyan,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.CUE4Parse => ConsoleColor.Magenta,
                LogLevel.Xmpp => ConsoleColor.DarkYellow
            };

            Console.Write(level.GetDescription());
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"] {message}");
        }

        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
                return null;

            var field = type.GetField(name);
            if (field == null)
                return null;

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                return attr.Description;

            return null;
        }
    }

    public enum LogLevel
    {
        [Description("INF0")] Info,

        [Description("CUE4")] CUE4Parse,

        [Description("ERORR")] Error,

        [Description("XMPP")] Xmpp,
    }
}
