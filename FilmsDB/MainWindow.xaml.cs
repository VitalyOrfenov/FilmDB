using System.Windows;
using System.Windows.Controls;
using System;

namespace FilmsDB
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            if (Convert.ToString(but.Content) == "Film Browser")
            {
                Watch_list.Visibility = Visibility.Collapsed;
                Movie_list.Visibility = Visibility.Visible;
            }
            else
            {
                Watch_list.Visibility = Visibility.Visible;
                Movie_list.Visibility = Visibility.Collapsed;
            }

        }

    }
}
