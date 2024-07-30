using System.Net;
using Telnet.Net;

public class Program
{
    public static void Main()
    {
        TelnetServer server = new TelnetServer(IPAddress.Parse("127.0.0.1"),
            8080,
            "admin",
            "password");
        
        server.Start();
    }
}