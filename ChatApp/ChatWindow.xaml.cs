using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatApp;

public partial class ChatWindow : Window
{
    String ip;
    String port;
    Boolean hosting;
    int messageCount = 0;
    Semaphore mutex; //Semaphore used to keep race conditions from happening
    String senderName = "Sender"; //Name of the other person.

    //Function to create a server host
    public void hostServer()
    {
        Task.Run(() =>
        {
            try
            {
                int portNum = int.Parse(this.port);
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, portNum);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine("Waiting for a connection on port " + portNum);
                // This blocking call runs on a background thread.
                Socket client = listener.Accept();
                Console.WriteLine("Client connected.");
                // Store the connection socket in the Tag property.
                Dispatcher.Invoke(() => { this.Tag = client; });
                // Start receiving messages on this connection.
                StartReceiving(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error hosting server: " + ex.Message);
            }
        });
    }

    //Function to connect to an existing host
    public void connectToServer()
    {
        Task.Run(() =>
        {
            try
            {
                if (!IPAddress.TryParse(this.ip, out IPAddress remoteIP))
                {
                    Console.WriteLine("Invalid IP address.");
                    return;
                }
                int portNum = int.Parse(this.port);
                IPEndPoint remoteEP = new IPEndPoint(remoteIP, portNum);
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Connecting to server at " + this.ip + ":" + portNum);
                clientSocket.Connect(remoteEP);
                Console.WriteLine("Connected to server.");
                Dispatcher.Invoke(() => { this.Tag = clientSocket; });
                // Start receiving messages on this connection.
                StartReceiving(clientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to server: " + ex.Message);
            }
        });
    }

    //Function to send a message to the other person
    public void sendMessage(String message)
    {
        try
        {
            if (this.Tag is Socket connectionSocket && connectionSocket.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                connectionSocket.Send(data);
            }
            else
            {
                Console.WriteLine("No valid connection to send message.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: " + ex.Message);
        }
    }

    //Helper method to start receiving messages in a background task.
    private void StartReceiving(Socket connection)
    {
        Task.Run(() =>
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = connection.Receive(buffer);
                    if (bytesRead <= 0)
                    {
                        // Connection closed.
                        break;
                    }
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Update UI on the dispatcher thread.
                    Dispatcher.Invoke(() => { addRecievedMessage(receivedMessage); });
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket error receiving message: " + se.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message: " + ex.Message);
            }
        });
    }

    //Function to add a sent message to the screen
    public void addSentMessage(String message)
    {
        Console.WriteLine("Before semaphore. Message: " + message);
        mutex.WaitOne();
        Console.WriteLine("In semaphore.");
        this.messageCount++;

        //Add two row definitions to the chat messages grid
        RowDefinition rowDefinition = new RowDefinition();
        rowDefinition.Height = GridLength.Auto;
        ChatMessages.RowDefinitions.Add(rowDefinition);
        rowDefinition = new RowDefinition();
        rowDefinition.Height = GridLength.Auto;
        ChatMessages.RowDefinitions.Add(rowDefinition);

        //Add the name label
        Label nameLabel = new Label();
        nameLabel.Content = "You";
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(nameLabel, 2);
        Grid.SetColumnSpan(nameLabel, 1);
        Grid.SetRow(nameLabel, (this.messageCount - 1) * 2);
        ChatMessages.Children.Add(nameLabel);

        //Add the message
        Border border = new Border();
        border.BorderThickness = new Thickness(1);
        border.BorderBrush = new SolidColorBrush(Colors.Black);
        Grid.SetColumn(border, 1);
        Grid.SetColumnSpan(border, 2);
        Grid.SetRow(border, (this.messageCount - 1) * 2 + 1);
        border.HorizontalAlignment = HorizontalAlignment.Right;

        TextBlock messageLabel = new TextBlock();
        messageLabel.Text = message;
        messageLabel.TextWrapping = TextWrapping.Wrap;
        messageLabel.Padding = new Thickness(5);

        border.Child = messageLabel;

        ChatMessages.Children.Add(border);

        mutex.Release();
        Console.WriteLine("Exiting semaphore.");
    }

    //Function to add a received message to the screen
    public void addRecievedMessage(String message)
    {
        mutex.WaitOne();
        this.messageCount++;

        //Add two row definitions to the chat messages grid
        RowDefinition rowDefinition = new RowDefinition();
        rowDefinition.Height = GridLength.Auto;
        ChatMessages.RowDefinitions.Add(rowDefinition);
        rowDefinition = new RowDefinition();
        rowDefinition.Height = GridLength.Auto;
        ChatMessages.RowDefinitions.Add(rowDefinition);

        //Add the name label
        Label nameLabel = new Label();
        nameLabel.Content = this.senderName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(nameLabel, 0);
        Grid.SetColumnSpan(nameLabel, 1);
        Grid.SetRow(nameLabel, (this.messageCount - 1) * 2);
        ChatMessages.Children.Add(nameLabel);

        //Add the message
        Border border = new Border();
        border.BorderThickness = new Thickness(1);
        border.BorderBrush = new SolidColorBrush(Colors.Black);
        Grid.SetColumn(border, 0);
        Grid.SetColumnSpan(border, 2);
        Grid.SetRow(border, (this.messageCount - 1) * 2 + 1);
        border.HorizontalAlignment = HorizontalAlignment.Left;

        TextBlock messageLabel = new TextBlock();
        messageLabel.Text = message;
        messageLabel.TextWrapping = TextWrapping.Wrap;
        messageLabel.Padding = new Thickness(5);

        border.Child = messageLabel;

        ChatMessages.Children.Add(border);

        mutex.Release();
    }

    public ChatWindow(String ip, String port, Boolean hosting)
    {
        Console.WriteLine("Constructor Entry");
        InitializeComponent();
        this.ip = ip;
        this.port = port;
        this.hosting = hosting;
        this.mutex = new Semaphore(initialCount: 1, maximumCount: 2);

        if (this.hosting)
        {
            hostServer();
        }
        else
        {
            connectToServer();
        }

        Console.WriteLine("Constructor Successful");
    }

    //Function to handle send message click. Sends a message over the network, adds a message to the screen, and then clears the text box that enters a message
    private void SendMessageBtClick(object sender, RoutedEventArgs e)
    {
        String message = ChatMessage.Text;
        sendMessage(message);
        addSentMessage(message);
        ChatMessage.Text = "";
        Scroller.ScrollToBottom();
    }
}
