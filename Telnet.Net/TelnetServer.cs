using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Telnet.Net;

public class TelnetServer
{
    private readonly TcpListener _listener;
    private readonly string Username;
    private readonly string Password;
    
    public TelnetServer(IPAddress ip, int port, string username, string password)
    {
        _listener = new TcpListener(ip, port);
        Username = username;
        Password = password;
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("telnet server is started...");

        while (true)
        {
            TcpClient client = _listener.AcceptTcpClient();
            Console.WriteLine($"client from: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()} is connected");
            var clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
        StreamReader reader = new StreamReader(stream);

        if (!AuthenticateClient(writer, reader))
        {
            client.Close();
            return;
        }

        writer.WriteLine("Welcome to the Telnet server!");

        bool connected = true;
        while (connected)
        {
            try
            {
                writer.Write("$ ");
                string command = reader.ReadLine();
                if (command == null || command.ToLower() == "exit")
                {
                    connected = false;
                    writer.WriteLine("Goodbye!");
                }
                else
                {
                    ExecuteCommand(writer, command);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                connected = false;
            }
        }

        client.Close();
        Console.WriteLine("Client disconnected...");
    }

    private void ExecuteCommand(StreamWriter writer, string command)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe", 
                Arguments = "-c \"" + command + "\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                writer.WriteLine(output);
            if (!string.IsNullOrWhiteSpace(error))
                writer.WriteLine("Error: " + error);
        }
        catch (Exception ex)
        {
            writer.WriteLine("Command execution failed: " + ex.Message);
        }
    }

    private bool AuthenticateClient(StreamWriter writer, StreamReader reader)
    {
        writer.WriteLine("login :");
        var username = reader.ReadLine();
        writer.WriteLine("password :");
        var password = reader.ReadLine();
        
        if (username == Username && password == Password)
        {
            return true;
        }
        else
        {
            writer.WriteLine("Authentication failed.");
            return false;
        }
    }
}
