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
using System.Windows.Shapes;

namespace Pong
{
    /// <summary>
    /// Interaction logic for UsageWindow.xaml
    /// This class is used for the usage window explaining the features
    /// of this rendition of the game of Pong.
    /// </summary>
    public partial class UsageWindow : Window
    {
        public UsageWindow()
        {
            InitializeComponent();

            string s = "\n\t---Game of Pong Usage---";
            s += "\nUpon startup the user can use either" +
                "\nthe menu bar, keyboard shortcuts, or" +
                "\nbuttons on the window to interact with" +
                "\nthe game. Once the game is started the" +
                "\nuser can then reset,pause/resume" +
                "\nthe game. The user can also mute the" +
                "\nsound. The user paddle is coordinated" +
                "\nwith mouse movement in the window." +
                "\n\n\t---Keyboard Shortcuts---" +
                "\nESCAPE      \t --> Pause/Resume" +
                "\nENTER        \t--> Start" +
                "\nUp/Down Arrows --> Move Paddle" +
                "\nCONTROL+M      --> Mute" +
                "\nControl+R   \t--> Reset"+
                "\nCONTROL+W      --> Game Cheat";
              

            lblUsageInfo.Content = s;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
