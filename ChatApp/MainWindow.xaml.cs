using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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
    }

    private void ConnectBtn_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Connect btn clicked " + IPAddress.Text + ":" + Port.Text);
    }

    private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = NUMBER.IsMatch(e.Text);
    }
}