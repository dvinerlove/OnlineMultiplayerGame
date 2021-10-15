using System;

namespace Project2
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            using (Game1 game = new())
            {
                //var game = new Game1();
                //game.SetAddress(AddressCB.SelectedItem.ToString() + ":" + PortTB.Text);
                //game.SetUsername(UsernameTB.Text);
                game.Run();
            }
        }
    }
}
