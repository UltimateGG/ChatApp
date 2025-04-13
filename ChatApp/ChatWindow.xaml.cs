using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatApp;

public partial class ChatWindow : Window
{
    String ip;
    String port;
    Boolean hosting;
    int messageCount = 0;
    Semaphore mutex; //Semaphore used to keep race conditions from happening
    String senderName = "Sender"; //Name of the other person. Defaults to sender, but could be set the ip:portnumber of the other person when the connection is made

    //Function to create a server host
    public void hostServer()
    {
        return;
    }

    //Function to connect to an exiting host
    public void connectToServer()
    {
        return;
    }

    //Function to send a message to other person
    public void sendMessage(String message)
    {
        return;
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

    //Function to add a recieved message to the screen
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
        this.ip = ip;
        this.port = port;
        this.hosting = hosting;
        this.mutex = new Semaphore(initialCount: 1, maximumCount: 2);

        if(this.hosting)
        {
            hostServer();
        } else
        {
            connectToServer();
        }

        InitializeComponent();
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
