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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Arctium_ClientDB_Viewer.Controls
{
    public class FileListItem : Border
    {
        public string Caption { get; set; }

        public FileListItem(string captionText, string imagePath) : base()
        {
            Margin = new Thickness(13);

            var panel = new StackPanel();
            var imageSource = new BitmapImage();
            var image = new Image();
            var caption = new TextBlock();

            imageSource.BeginInit();
            imageSource.UriSource = new Uri(imagePath, UriKind.Relative);
            imageSource.EndInit();

            image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            image.Source = imageSource;
            image.Height = 64;
            image.Width = 64;
            image.ToolTip = "Select & click on 'Table' for data view.";

            caption.TextAlignment = TextAlignment.Center;
            caption.TextWrapping = TextWrapping.Wrap;
            caption.Text = captionText;

            Caption = captionText;

            if (caption.Text.Length <= 18)
                caption.Text += "\n ";

            panel.Children.Add(image);
            panel.Children.Add(caption);

            Child = panel;
        }
    }
}
