using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FenceNote.Models;
using FenceNote.ViewModels;

namespace FenceNote
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.DarkModeRequested += ViewModel_DarkModeRequested;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerInitialTheme();

            _viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.IsVaultPromptOpen))
                {
                    if (_viewModel.IsVaultPromptOpen)
                    {
                        VaultPasswordBox.Clear();
                        VaultConfirmPasswordBox.Clear();
                    }
                }
                else if (args.PropertyName == nameof(MainViewModel.IsUnlockPromptOpen))
                {
                    if (_viewModel.IsUnlockPromptOpen)
                    {
                        UnlockVaultPasswordBox.Clear();
                        UnlockVaultPasswordBox.Focus();
                    }
                }
            };
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            _viewModel.ForceSavePendingChanges();
        }

        private void ViewModel_DarkModeRequested(object? sender, bool isDarkMode)
        {
            Color windowBg = isDarkMode ? (Color)ColorConverter.ConvertFromString("#1E1E1E") : (Color)ColorConverter.ConvertFromString("#F5F5F5");
            Color pane1Bg = isDarkMode ? (Color)ColorConverter.ConvertFromString("#252526") : (Color)ColorConverter.ConvertFromString("#DCDCDC");
            Color pane2Bg = isDarkMode ? (Color)ColorConverter.ConvertFromString("#2D2D30") : (Color)ColorConverter.ConvertFromString("#EAEAEA");
            Color pane3Bg = isDarkMode ? (Color)ColorConverter.ConvertFromString("#1E1E1E") : (Color)ColorConverter.ConvertFromString("#FFFFFF");
            Color textPrimary = isDarkMode ? (Color)ColorConverter.ConvertFromString("#D4D4D4") : (Color)ColorConverter.ConvertFromString("#000000");
            Color textSecondary = isDarkMode ? (Color)ColorConverter.ConvertFromString("#858585") : (Color)ColorConverter.ConvertFromString("#888888");
            Color border = isDarkMode ? (Color)ColorConverter.ConvertFromString("#3F3F46") : (Color)ColorConverter.ConvertFromString("#CCCCCC");
            Color textCaret = isDarkMode ? (Color)ColorConverter.ConvertFromString("#FFFFFF") : (Color)ColorConverter.ConvertFromString("#000000");

            AnimateColorResource("WindowBgBrush", windowBg);
            AnimateColorResource("Pane1BgBrush", pane1Bg);
            AnimateColorResource("Pane2BgBrush", pane2Bg);
            AnimateColorResource("Pane3BgBrush", pane3Bg);
            AnimateColorResource("TextPrimaryBrush", textPrimary);
            AnimateColorResource("TextSecondaryBrush", textSecondary);
            AnimateColorResource("BorderBrush", border);
            AnimateColorResource("TextCaretBrush", textCaret);
        }

        private void AnimateColorResource(string resourceKey, Color toColor)
        {
            if (Resources[resourceKey] is SolidColorBrush oldBrush)
            {
                var newBrush = new SolidColorBrush(oldBrush.Color);

                var animation = new ColorAnimation
                {
                    To = toColor,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                Resources[resourceKey] = newBrush;
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as DependencyObject;

            while (clickedElement != null)
            {
                if (clickedElement is TextBox) return;
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            foreach (var vault in _viewModel.Vaults)
            {
                if (vault.IsEditing) _viewModel.CancelVaultRename(vault);
            }
            foreach (var note in _viewModel.DisplayedNotes)
            {
                if (note.IsEditing) _viewModel.CancelNoteRename(note);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.ResetIdleTimer();
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.DataContext is Vault vault)
                {
                    _viewModel.CommitVaultRename(vault, textBox.Text);
                }
                else if (textBox.DataContext is Note note)
                {
                    _viewModel.CommitNoteRename(note, textBox.Text);
                }
            }
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Enter)
                {
                    if (textBox.DataContext is Vault vault)
                    {
                        _viewModel.CommitVaultRename(vault, textBox.Text);
                    }
                    else if (textBox.DataContext is Note note)
                    {
                        _viewModel.CommitNoteRename(note, textBox.Text);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    if (textBox.DataContext is Vault vault)
                    {
                        _viewModel.CancelVaultRename(vault);
                    }
                    else if (textBox.DataContext is Note note)
                    {
                        _viewModel.CancelNoteRename(note);
                    }
                    e.Handled = true;
                }
            }
        }

        private void VaultPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.VaultPasswordInput = VaultPasswordBox.Password;
            }
        }

        private void VaultConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.VaultConfirmPasswordInput = VaultConfirmPasswordBox.Password;
            }
        }

        private void UnlockVaultPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.UnlockPasswordInput = UnlockVaultPasswordBox.Password;
            }
        }

        private void UnlockVaultPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is MainViewModel vm)
            {
                if (vm.ConfirmUnlockVaultCommand.CanExecute(null))
                {
                    vm.ConfirmUnlockVaultCommand.Execute(null);
                }
            }
        }
    }
}