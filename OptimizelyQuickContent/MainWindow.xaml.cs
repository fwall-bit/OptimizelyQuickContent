using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace OptimizelyQuickContent {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ObservableCollection<ConnectionInfo> _connections = new();
        private readonly ConnectionService _connectionService = new();
        private string? _accessToken;
        private bool _isEditing;

        public MainWindow() {
            InitializeComponent();

            foreach (var connection in ConnectionStore.Load())
                _connections.Add(connection);

            ConnectionsComboBox.ItemsSource = _connections;
        }

        private void ConnectionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = ConnectionsComboBox.SelectedItem as ConnectionInfo;
            ConnectButton.IsEnabled = selected is not null;
            RequestPanel.Visibility = Visibility.Collapsed;
            _accessToken = null;
            StatusText.Text = selected is not null
                ? $"Selected: {selected.DisplayName}"
                : "No connection selected.";
        }

        private async void Connect_Click(object sender, RoutedEventArgs e) {
            ConnectButton.IsEnabled = false;
            var selected = ConnectionsComboBox.SelectedItem as ConnectionInfo;
            if (selected is null) {
                MessageBox.Show("Please select a connection first.", "No Connection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConnectButton.IsEnabled = true;
                return;
            }

            StatusText.Text = "Requesting access token..";
            try {
                _accessToken = await _connectionService.GetAccessTokenAsync(
                    selected.BaseUrl, selected.ClientId, selected.ClientSecret);

                StatusText.Text = "Connection successful!";
                RequestPanel.Visibility = Visibility.Visible;
            } catch (Exception ex) {
                _accessToken = null;
                StatusText.Text = $"Connection failed: {ex.Message}";
                RequestPanel.Visibility = Visibility.Collapsed;
            } finally {
                ConnectButton.IsEnabled = true;
            }
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e) {
            _isEditing = false;
            DisplayNameTextBox.Text = string.Empty;
            BaseUrlTextBox.Text = string.Empty;
            ClientIdTextBox.Text = string.Empty;
            ClientSecretTextBox.Text = string.Empty;
            ConnectionFormPanel.Visibility = Visibility.Visible;
        }

        private void EditConnection_Click(object sender, RoutedEventArgs e) {
            _isEditing = true;
            if (ConnectionsComboBox.SelectedItem is not ConnectionInfo selected) return;

            DisplayNameTextBox.Text = selected.DisplayName;
            BaseUrlTextBox.Text = selected.BaseUrl;
            ClientIdTextBox.Text = selected.ClientId;
            ClientSecretTextBox.Text = selected.ClientSecret;
            ConnectionFormPanel.Visibility = Visibility.Visible;
        }

        private void RemoveConnection_Click(object sender, RoutedEventArgs e) {
            if (ConnectionsComboBox.SelectedItem is not ConnectionInfo selected) return;

            var result = MessageBox.Show($"Are you sure you want to remove the connection \"{selected.DisplayName}\"?",
                "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) {
                ConnectionsComboBox.SelectedItem = null;
                _connections.Remove(selected);
                ConnectionStore.Save(_connections);
                StatusText.Text = $"Connection \"{selected.DisplayName}\" removed.";
            }
        }

        private void CancelForm_Click(object sender, RoutedEventArgs e) {
            DisplayNameTextBox.Text = string.Empty;
            BaseUrlTextBox.Text = string.Empty;
            ClientIdTextBox.Text = string.Empty;
            ClientSecretTextBox.Text = string.Empty;
            ConnectionFormPanel.Visibility = Visibility.Collapsed;
        }

        private void SaveConnection_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(BaseUrlTextBox.Text) ||
                string.IsNullOrWhiteSpace(ClientIdTextBox.Text) ||
                string.IsNullOrWhiteSpace(ClientSecretTextBox.Text)) {
                MessageBox.Show("All fields are required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var connection = new ConnectionInfo {
                DisplayName = DisplayNameTextBox.Text.Trim(),
                BaseUrl = BaseUrlTextBox.Text.Trim(),
                ClientId = ClientIdTextBox.Text.Trim(),
                ClientSecret = ClientSecretTextBox.Text.Trim()
            };

            if (_isEditing) {
                if (ConnectionsComboBox.SelectedItem is not ConnectionInfo selected) return;
                selected.DisplayName = connection.DisplayName;
                selected.BaseUrl = connection.BaseUrl;
                selected.ClientId = connection.ClientId;
                selected.ClientSecret = connection.ClientSecret;
                _isEditing = false;
            } else {
                _connections.Add(connection);
            }
            ConnectionStore.Save(_connections);
            ConnectionsComboBox.SelectedItem = connection;

            ConnectionFormPanel.Visibility = Visibility.Collapsed;
            StatusText.Text = $"Connection \"{connection.DisplayName}\" saved.";
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e) {
            var selected = ConnectionsComboBox.SelectedItem as ConnectionInfo;
            if (selected is null || string.IsNullOrEmpty(_accessToken)) {
                MessageBox.Show("Please connect first.", "Not Connected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(RequestBodyTextBox.Text)) {
                MessageBox.Show("Request body cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusText.Text = "Sending request...";
            try {
                var response = await _connectionService.CreateContentAsync(
                    selected.BaseUrl, _accessToken, RequestBodyTextBox.Text);

                StatusText.Text = "Content created successfully!";
                ResponseHeader.Visibility = Visibility.Visible;
                ResponseTextBox.Visibility = Visibility.Visible;
                ResponseTextBox.Text = response;
            } catch (Exception ex) {
                StatusText.Text = $"Request failed: {ex.Message}";
                ResponseHeader.Visibility = Visibility.Visible;
                ResponseTextBox.Visibility = Visibility.Visible;
                ResponseTextBox.Text = ex.Message;
            }
        }

        private void CloseResponse_Click(object sender, RoutedEventArgs e) {
            ResponseHeader.Visibility = Visibility.Collapsed;
            ResponseTextBox.Visibility = Visibility.Collapsed;
            ResponseTextBox.Text = string.Empty;
        }
    }
}