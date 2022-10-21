using System.Windows;

namespace VKApplication.App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void listItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((System.Windows.Controls.ListBox)sender).SelectedItem != null)
            Model.AudioService.GetInstance().StartPlay((Model.Item)((System.Windows.Controls.ListBox)sender).SelectedItem);
        }
    }
}
