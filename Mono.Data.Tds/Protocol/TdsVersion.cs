namespace Mono.Data.Tds.Protocol
{
    public enum TdsVersion
    {
        tds42 = 42,     // used by older Sybase and Microsoft SQL (< 7.0) servers
        tds50 = 50,     // used by Sybase
        tds70 = 70,     // used by Microsoft SQL server 7.0/2000
        tds80 = 80,     // used by Microsoft SQL server 2000
        tds81 = 81,     // used by Microsoft SQL server 2000 SP1
        tds90 = 90,     // used by Microsoft SQL server 2005
        tds100 = 100    // used by Microsoft SQL server 2008
    }
}
