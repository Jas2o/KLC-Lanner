using System;
using System.Windows;

namespace KLC_Hawk {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window {

        public WindowShark WindowSharkCapture;

        public WindowMain() {
            App.Hawk = new Hawk(this);
            this.DataContext = App.Hawk;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            menuStartCapture_Click(sender, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Environment.Exit(0);
        }

        #region Log
        private void menuLogClear_Click(object sender, RoutedEventArgs e) {
            txtLog.Clear();
        }
        #endregion

        #region Capture
        private void menuStartCapture_Click(object sender, RoutedEventArgs e) {
            if (WindowSharkCapture != null && WindowSharkCapture.IsVisible)
                return;

            WindowSharkCapture = new WindowShark(true);
            WindowSharkCapture.Show();
        }

        private void menuOpenCapture_Click(object sender, RoutedEventArgs e) {
            new WindowShark().Show();
        }

        private void menuFilterLC_Click(object sender, RoutedEventArgs e) {
            string filter = App.Hawk.GetWiresharkFiltersLiveConnect();
            if (filter != "")
                Clipboard.SetText(filter);
        }
        #endregion

        private void menuDropY_Click(object sender, RoutedEventArgs e)
        {
            App.Hawk.DropY();
        }

        private void menuDropZ_Click(object sender, RoutedEventArgs e)
        {
            App.Hawk.DropZ();
        }

    }
}
