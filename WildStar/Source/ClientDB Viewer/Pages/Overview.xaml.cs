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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arctium_ClientDB_Viewer.Controls;
using Framework;
using Microsoft.Win32;

namespace Arctium_ClientDB_Viewer.Pages
{
    public partial class Overview : Page
    {
        public Overview()
        {
            InitializeComponent();

            if (!Globals.Refresh)
            {
                LoadFilesIntoGrid();

                FileView.Visibility = Visibility.Visible;
                FileImg.Visibility = Visibility.Hidden;

                SetTextVisibility(Visibility.Hidden);
            }
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetTextVisibility(Visibility.Visible);
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            SetTextVisibility(Visibility.Hidden);
        }

        private void FileImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.DefaultExt = ".tbl";
            fileDialog.Filter = "WildStar Database Files (.tbl)|*.tbl";

            if (fileDialog.ShowDialog() == true)
                LoadFiles(fileDialog.FileNames);
        }

        private void File_PreviewDrop(object sender, DragEventArgs e)
        {
            var filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            LoadFiles(filenames);

            e.Handled = true;
        }

        void LoadFiles(string[] files)
        {
            Parallel.ForEach(files, f =>
            {
                var fileInfo = new FileInfo(f);
                
                Globals.DBFiles.TryAdd(fileInfo.Name, f);
            });

            LoadFilesIntoGrid();
        }

        void LoadFilesIntoGrid()
        {
            var list = new List<FileListItem>();
            var sortedItems = Globals.DBFiles.Keys.ToList();

            sortedItems.Sort();

            for (int i = 0; i < sortedItems.Count; i++)
                list.Add(new FileListItem(sortedItems[i], "/Arctium ClientDB Viewer;component/Images/tbl.png"));

            FileView.ItemsSource = list;
            FileView.Visibility = Visibility.Visible;

            Globals.Refresh = false;
        }

        void SetTextVisibility(Visibility v)
        {
            Click.Visibility = v;
            Or.Visibility = v;
            DragnDrop.Visibility = v;
            YFiles.Visibility = v;
        }

        void FileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.SelectedItem = (e.AddedItems[0] as FileListItem).Caption;
        }
    }
}
