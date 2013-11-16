/*
 * Copyright (C) 2012-2013 Arctium Emulation <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Arctium_ClientDB_Viewer.Reader;
using Framework;

namespace Arctium_ClientDB_Viewer.Pages
{
    public partial class Table : Page
    {
        public Table()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                string file;

                if (Globals.DBFiles.TryGetValue(Globals.SelectedItem, out file))
                {

                    if (!Globals.DBTables.ContainsKey(Globals.SelectedItem))
                        Dispatcher.Invoke(new Action(() => DBReader.Read(file, Dispatcher.CurrentDispatcher, ProgressBar, DBData)), DispatcherPriority.Normal);
                    else
                    {
                        DataTable table;

                        if (Globals.DBTables.TryGetValue(Globals.SelectedItem, out table))
                            Dispatcher.Invoke(new Action(() => DBData.ItemsSource = table.DefaultView), DispatcherPriority.Normal);
                    }
                }
            });
        }
    }
}
