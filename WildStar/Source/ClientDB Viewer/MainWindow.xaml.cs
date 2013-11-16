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
using System.Windows.Input;
using System.Windows.Media;

namespace Arctium_ClientDB_Viewer
{
    public partial class MainWindow : Window
    {
        TextBlock selectedBlock;
        Point currentPosition;
        bool canMove;

        public MainWindow()
        {
            InitializeComponent();

            MinimizeButton.Click += (o, e) => WindowState = WindowState.Minimized;
            var marginBackup = MenuGrid.Margin;

            MaximizeRestoreButton.Click += (o, e) =>
            {
                if (WindowState == WindowState.Normal)
                {
                    BorderThickness = new Thickness(7, 0, 7, 0);

                    var right = MenuGrid.Margin.Right;
                    MenuGrid.Margin = new Thickness(0, 0, right, Header.Height * 0.75);

                    WindowState = WindowState.Maximized;
                }
                else
                {
                    BorderThickness = new Thickness(1.4);

                    MenuGrid.Margin = marginBackup;

                    WindowState = WindowState.Normal;
                }
            };

            CloseButton.Click += (o, e) => Environment.Exit(0);

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - SystemParameters.WindowResizeBorderThickness.Top - SystemParameters.WindowResizeBorderThickness.Bottom;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            Top = 0;
            Left = 0;

            TextBlock_MouseLeftButtonUp(OverViewBlock, null);
        }

        /// <summary>
        /// MouseMove event.
        /// Gets the new mouse position and handles the window movement.
        /// </summary>
        void SetNewWindowPosition(object sender, MouseEventArgs e)
        {
            if (!canMove)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var newPos = e.GetPosition(this);

                Left -= currentPosition.X - newPos.X;
                Top -= currentPosition.Y - newPos.Y;
            }
        }

        /// <summary>
        /// MouseDown event.
        /// Gets the current mouse position on left mouse button click.
        /// </summary>
        void SetCurrentMousePosition(object sender, MouseButtonEventArgs e)
        {
            canMove = false;

            if (!Header.IsMouseOver)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                canMove = true;

                currentPosition = e.GetPosition(this);
            }
        }

        void DisableCanMove(object sender, MouseButtonEventArgs e)
        {
            canMove = false;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedBlock != null)
            {
                selectedBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x2C, 0x30));
                selectedBlock.IsEnabled = true;
            }

            var currentBlock = sender as TextBlock;

            MainFrame.Navigate(new Uri("Pages/" + currentBlock.Text + ".xaml", UriKind.Relative));

            currentBlock.Foreground = new SolidColorBrush(Color.FromArgb(0x7F, 0x0B, 0xB1, 0xFF));
            currentBlock.IsEnabled = false;

            selectedBlock = currentBlock;
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            var currentBlock = sender as TextBlock;

            if (currentBlock.IsEnabled)
                currentBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0xDD, 0xFF));
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            var currentBlock = sender as TextBlock;

            if (currentBlock.IsEnabled && selectedBlock != currentBlock)
                currentBlock.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x2C, 0x30));
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 222, 106, 91));
            BorderThickness = new Thickness(1.4);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x29, 0xC4, 0xF1));
            BorderThickness = new Thickness(1.4);
        }
    }
}
