namespace System.Data
{
    internal class DiagnosticListener
    {
        internal static bool DiagnosticListenerEnabled = false;
        internal DiagnosticListener(string s) { }
        internal bool IsEnabled(string s) => DiagnosticListenerEnabled;
        internal void Write(string s1, object s2) { System.Console.WriteLine($"|| {s1},  {s2}"); }
    }
}
