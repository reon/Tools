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
using System.Data;
using System.Reflection;
using System.IO;

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

            foreach (string dll in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories))
                Assembly.LoadFile(dll);

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

            Globals.Conn.Database.Ext().StoredClasses().ToList().ForEach(sClass =>
            {
                ObjectList.Items.Add(sClass.GetName());
            });
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

        private void ObjectList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataTable dt = new DataTable();
            DataColumn column = null;
            DataRow row = null;

            var arr = Globals.Conn.Select(Type.GetType(Globals.Conn.Database.Ext().StoredClass(ObjectList.SelectedValue).GetName()));

            Globals.Conn.Database.Ext().StoredClass(ObjectList.SelectedValue).GetStoredFields().ToList().ForEach(field =>
            {
                string name = field.GetName().Split(new char[] { '>' })[0].Replace("<", "").Trim();

                column = new DataColumn(name);
                dt.Columns.Add(column);
            });


            for (int i = 0; i < arr.Length; i++)
            {
                row = dt.NewRow();

                foreach (DataColumn c in dt.Columns)
                {
                    Globals.Conn.Database.Ext().StoredClass(ObjectList.SelectedValue).GetStoredFields().ToList().ForEach(field =>
                    {
                        string name = field.GetName().Split(new char[] { '>' })[0].Replace("<", "").Trim();

                        if (c.ColumnName == name)
                            row[name] = field.Get(arr.GetValue(i));
                    });
                }

                dt.Rows.Add(row);
            }

            ObjectFieldTable.ItemsSource = dt.DefaultView;
        }
    }
}
