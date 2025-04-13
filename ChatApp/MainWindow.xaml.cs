using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ChatApp;

namespace ChatApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static Regex NUMBER = new Regex("[^0-9]+");

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Keyboard.ClearFocus();
    }


    private void StartServerBtn_Click(object sender, RoutedEventArgs e)
    {

        Debug.WriteLine("Start server btn clicked " + IPAddress.Text + ":" + Port.Text);
        ChatWindow chatWindow = new ChatWindow(IPAddress.Text, Port.Text, true);
        chatWindow.Owner = this;
        chatWindow.Show();
        Console.WriteLine("Create Host successfully");
    }

    private void ConnectBtn_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Connect btn clicked " + IPAddress.Text + ":" + Port.Text);
        ChatWindow chatWindow = new ChatWindow(IPAddress.Text, Port.Text, false);
        chatWindow.Owner = this;
        chatWindow.Show();
        Console.WriteLine("Create client succesful");
    }

    private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = NUMBER.IsMatch(e.Text);
    }
}