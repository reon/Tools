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

namespace Arctium_Database_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Globals.Conn == null)
                DisableButtonsOnDisconnect();
        }

        private void MainExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            new ConnectWindow().ShowDialog();

            DisconnectButton.IsEnabled = true;
            ObjectAddButton.IsEnabled = true;
            ObjectRemoveButton.IsEnabled = true;

            foreach (var sClass in Globals.Conn.Database.Ext().StoredClasses())
                if (sClass.InstanceCount() > 0)
                    ObjectList.Items.Add(sClass.GetName());
        }

        void DisableButtonsOnDisconnect()
        {
            DisconnectButton.IsEnabled = false;
            ObjectAddButton.IsEnabled = false;
            ObjectRemoveButton.IsEnabled = false;
            ObjectEditButton.IsEnabled = false;
            ObjectSaveButton.IsEnabled = false;
            ObjectUndoButton.IsEnabled = false;
            ObjectRedoButton.IsEnabled = false;
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.Conn != null)
                Globals.Conn.Database.Close();

            DisableButtonsOnDisconnect();
        }
    }
}
