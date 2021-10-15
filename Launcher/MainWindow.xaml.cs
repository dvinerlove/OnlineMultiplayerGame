using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project2;
using NameGenerator.Generators;
using ServerApp;
using System.Diagnostics;
using System.Net;
using RandomFriendlyNameGenerator;

namespace Louncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetName();


            string myHost = System.Net.Dns.GetHostName();
            Debug.WriteLine(myHost);

            System.Net.IPHostEntry myIPs = System.Net.Dns.GetHostEntry(myHost);
            Debug.WriteLine(myIPs);

            // Loop through all IP addresses and display each 
            List<string> addresses = new();
            foreach (System.Net.IPAddress myIP in myIPs.AddressList)
            {
                if (myIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    addresses.Add(myIP.ToString());
                }
            }
            AddressCB.ItemsSource = addresses;
            AddressCB.SelectedIndex = 0;
        }

        private void SetName()
        {
            Random random = new Random();
            if (random.Next(100) > 50)
            {

                UsernameTB.Text = RandomFriendlyNameGenerator.NameGenerator.Identifiers.Get(50, IdentifierComponents.Adjective | IdentifierComponents.Noun | IdentifierComponents.Animal, separator: "_", lengthRestriction: 20).ToList()[4];
            }
            else
            {
                GamerTagGenerator tagGenerator = new();

                tagGenerator.SpaceCharacter = "_";
                UsernameTB.Text = tagGenerator.Generate();// DSharp.ContactGenerator.Generator.Username("aboba");
            }
        }

        [STAThread]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ServerApp.Program.Main(new string[] { PortTB.Text });
            //Process.GetProcessesByName("Server.exe").FirstOrDefault().Kill();

            Process.Start("Server.exe", PortTB.Text);
            //Process.Start("Project2.exe", AddressCB.SelectedItem.ToString() + ":" + PortTB.Text + " " + UsernameTB.Text);
            string address = AddressCB.SelectedItem.ToString() + ":" + PortTB.Text;
            string name = UsernameTB.Text;
            var game = new Game1();
            game.SetAddress(address);
            game.SetUsername(name);
            game.Run();
            this.Close();
        }
        [STAThread]
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var game = new Game1())
            {
                game.SetAddress(AddressTB.Text);
                game.SetUsername(UsernameTB.Text);
                game.Run();
            }
            this.Close();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SetName();
        }
    }
}
