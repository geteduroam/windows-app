﻿using System;
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

namespace WpfApp.Menu
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        private MainWindow mainWindow;
        public MainMenu(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            //btnExisting.Visibility = Visibility.Collapsed;
            LoadPage();
        }

        private void LoadPage()
        {
            mainWindow.btnNext.Visibility = Visibility.Hidden;
            mainWindow.btnBack.Visibility = Visibility.Hidden;
        }

        private void BtnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NextPage();
        }
    }
}