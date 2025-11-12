 using LLMConfigManager.DataAccess;
 using LLMConfigManager.BusinessLogic;
 using LLMConfigManager.Model;
 using Microsoft.Win32;
 using System.Collections.Generic;
 using System.Diagnostics;
 using System.IO;
 using System.Linq;
 using System.Windows;
 using System.Windows.Controls;
 using System.Windows.Navigation;
 
 namespace LLMConfigManager
 {
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
         private readonly ProfileManager _profileManager;
         private readonly SettingsManager _settingsManager;
         private List<ModelProfile> _profiles;
         private string _settingsJsonPath;
 
         public MainWindow()
         {
             InitializeComponent();
 
             _profileManager = new ProfileManager();
             _settingsManager = new SettingsManager();
             _profiles = new List<ModelProfile>();
 
             // Event Handlers
             NewButton.Click += NewButton_Click;
             DeleteButton.Click += DeleteButton_Click;
             SaveButton.Click += SaveButton_Click;
             BrowseSettingsButton.Click += BrowseSettingsButton_Click;
             ApplyButton.Click += ApplyButton_Click;
             ResetButton.Click += ResetButton_Click;
             LaunchWithProfileButton.Click += LaunchWithProfileButton_Click;
             LaunchDirectlyButton.Click += LaunchDirectlyButton_Click;
             ProfileListBox.SelectionChanged += ProfileListBox_SelectionChanged;

            LoadProfiles();
         }
 
         private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
         {
             Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
             e.Handled = true;
         }
 
         private void LoadProfiles()
         {
             _profiles = _profileManager.LoadProfiles();
             var selectedName = ProfileListBox.SelectedItem?.ToString();
             ProfileListBox.Items.Clear();
             foreach (var profile in _profiles) { ProfileListBox.Items.Add(profile.Name); }
             if (selectedName != null && ProfileListBox.Items.Contains(selectedName)) { ProfileListBox.SelectedItem = selectedName; }
             else { ClearSelection(); }
         }
 
         private void ProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             if (ProfileListBox.SelectedItem == null) return;
             var selectedName = ProfileListBox.SelectedItem.ToString();
             var selectedProfile = _profiles.FirstOrDefault(p => p.Name == selectedName);
             if (selectedProfile != null)
             {
                 ProfileNameTextBox.Text = selectedProfile.Name;
                 StatementBlockTextBox.Text = selectedProfile.StatementBlock;
             }
         }
 
         private void ClearSelection()
         {
             ProfileListBox.SelectedItem = null;
             ProfileNameTextBox.Text = "";
             StatementBlockTextBox.Text = "";
         }
 
         private void NewButton_Click(object sender, RoutedEventArgs e)
         {
             ClearSelection();
         }
 
         private void DeleteButton_Click(object sender, RoutedEventArgs e)
         {
             if (ProfileListBox.SelectedItem == null) { MessageBox.Show("请先在列表中选择一个要删除的配置。", "提示", MessageBoxButton.OK,
 MessageBoxImage.Information); return; }
             var selectedName = ProfileListBox.SelectedItem.ToString();
             var selectedProfile = _profiles.FirstOrDefault(p => p.Name == selectedName);
             if (selectedProfile != null)
             {
                 if (MessageBox.Show($"确定要删除配置 '{selectedName}' 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
 MessageBoxResult.Yes)
                 {
                     _profiles.Remove(selectedProfile);
                     _profileManager.SaveProfiles(_profiles);
                     LoadProfiles();
                 }
             }
         }
 
         private void SaveButton_Click(object sender, RoutedEventArgs e)
         {
             var profileName = ProfileNameTextBox.Text.Trim();
             if (string.IsNullOrWhiteSpace(profileName)) { MessageBox.Show("配置名称不能为空。", "错误", MessageBoxButton.OK, MessageBoxImage.Error); return;
 }
             var existingProfile = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, System.StringComparison.OrdinalIgnoreCase));
             if (existingProfile != null) { existingProfile.StatementBlock = StatementBlockTextBox.Text; }
             else { _profiles.Add(new ModelProfile { Name = profileName, StatementBlock = StatementBlockTextBox.Text }); }
             _profileManager.SaveProfiles(_profiles);
             var currentName = profileName;
             LoadProfiles();
             ProfileListBox.SelectedItem = currentName;
             MessageBox.Show($"配置 '{profileName}' 已保存。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
         }
 
         private void BrowseSettingsButton_Click(object sender, RoutedEventArgs e)
         {
             var openFileDialog = new OpenFileDialog
             {
                 Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                 Title = "选择 settings.json 文件"
             };
 
             if (openFileDialog.ShowDialog() == true)
             {
                 _settingsJsonPath = openFileDialog.FileName;
                 SettingsPathLabel.Content = _settingsJsonPath;
             }
         }
 
         private void ApplyButton_Click(object sender, RoutedEventArgs e)
         {
             if (string.IsNullOrWhiteSpace(_settingsJsonPath) || !File.Exists(_settingsJsonPath)) { MessageBox.Show("请先通过“浏览”按钮选择一个有效的 settings.json 文件。", "路径无效", MessageBoxButton.OK, MessageBoxImage.Error); return; }
             if (ProfileListBox.SelectedItem == null) { MessageBox.Show("请先在列表中选择一个要应用的配置。", "未选择配置", MessageBoxButton.OK,
 MessageBoxImage.Information); return; }
             var selectedName = ProfileListBox.SelectedItem.ToString();
             var selectedProfile = _profiles.FirstOrDefault(p => p.Name == selectedName);
             if (selectedProfile == null) return;
             var envVars = _settingsManager.ParseStatementBlock(selectedProfile.StatementBlock);
             var success = _settingsManager.ApplyToSettings(_settingsJsonPath, envVars);
             if (success) { MessageBox.Show($"成功将配置 '{selectedName}' 应用到\n{_settingsJsonPath}", "应用成功", MessageBoxButton.OK,
 MessageBoxImage.Information); }
             else { MessageBox.Show("应用配置失败。请检查文件路径、权限和 JSON 文件格式是否正确。", "应用失败", MessageBoxButton.OK, MessageBoxImage.Error);
 }
         }
 
         private void ResetButton_Click(object sender, RoutedEventArgs e)
         {
             if (string.IsNullOrWhiteSpace(_settingsJsonPath) || !File.Exists(_settingsJsonPath)) { MessageBox.Show("请先通过“浏览”按钮选择一个有效的 settings.json 文件。", "路径无效", MessageBoxButton.OK, MessageBoxImage.Error); return; }
 
             if (MessageBox.Show($"确定要重置 {_settingsJsonPath} 文件中的 'env' 配置吗？", "确认重置", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
 MessageBoxResult.Yes)
             {
                 var success = _settingsManager.ResetSettings(_settingsJsonPath);
                 if (success) { MessageBox.Show("重置成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information); }
                 else { MessageBox.Show("重置失败。请检查文件路径、权限和 JSON 文件格式是否正确。", "重置失败", MessageBoxButton.OK, MessageBoxImage.Error);
 }
             }
         }
 
         private void LaunchWithProfileButton_Click(object sender, RoutedEventArgs e)
         {
             if (ProfileListBox.SelectedItem == null) { MessageBox.Show("请先在列表中选择一个要用于启动的配置。", "未选择配置", MessageBoxButton.OK,
 MessageBoxImage.Information); return; }
             var selectedName = ProfileListBox.SelectedItem.ToString();
             var selectedProfile = _profiles.FirstOrDefault(p => p.Name == selectedName);
             if (selectedProfile == null) return;
             _settingsManager.LaunchClaudeWithProfile(selectedProfile);
         }
 
         private void LaunchDirectlyButton_Click(object sender, RoutedEventArgs e)
         {
             _settingsManager.LaunchClaudeDirectly();
         }
     }
 }