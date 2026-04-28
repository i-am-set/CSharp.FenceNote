lina6519
linabeena21
Sharing their screen

eth — 4/15/26, 7:52 PM
alright, i feel like we're having to decypher his emails half the time lmao
alright I redid the video so the right clicks were visible. here's the final product
https://youtu.be/_hAeQYNU9Q4
YouTube
Ethanael Gibson
Group #3 Sprint 2 4/16

set — 4/15/26, 8:02 PM
Dude awesome. That’s literally perfect
Yea you can go ahead and submit that
Huge!
set — 4/17/26, 4:57 PM
@smh99 @eth @lina6519 

Ok guys, this is a big deal. Looks like we have to submit a GitHub repo for our application at the end of the semester. 

This is a problem because obviously I’m the only one that has been committing anything, but I have a decent plan on how to maybe fix that. 

For those that don’t understand, a group that all worked on the project would each have submitted “changes” to the repository, but we haven’t been doing that.
To fake it, I’m just going to set up the repo at its current state, then I’m going to set up 1 on q calls with each of you and I’ll show you how to push the change you were assigned for the last sprint so your name is on it. Easy as that. Then there really shouldn’t be any question about it
Don’t worry about it right now, I’ll update yall when the time comes, just be expected to do some weird technical stuff that I’ll have to walk you through to make sure we all get credit 
set — 4/22/26, 4:52 PM
Heres the github https://github.com/i-am-set/CSharp.FenceNote
GitHub
GitHub - i-am-set/CSharp.FenceNote
Contribute to i-am-set/CSharp.FenceNote development by creating an account on GitHub.
I don't plan to do anything with it this weekend, but I'm just planning what I'll need us to do next week. Thought I'd share it with y'all in case you were curious
But the entire repo in imported and the release has been added for easy downloading, no need for the google drive installer
set — Yesterday at 10:58 AM
@smh99 @eth @lina6519 Alright guys, I’m gonna start trying to set up code changes that I will have you guys commit so your names are on the project. It should be quick for you guys to do when I’m ready for each of you, so let me know when this week you’re free and I’ll help you commit the changes
They you should be in the clear
set — Yesterday at 12:07 PM
@lina6519 I have the first commit ready; its a polishing and cleaning of the visuals. These changes will fit what you've been assigned to, so I'll have you be the first to commit changes. Let me know when you're available and well take 15 minutes, and I'll show you the few things you'll need to do
lina6519 — Yesterday at 10:43 PM
I can do it tomorrow around 5 pm if that works with you?
set — Yesterday at 10:44 PM
Yea, sounds good, Ill be on around then. It wont take long, ill jsut have to talk you through it
lina6519 — Yesterday at 10:44 PM
Okay!
I'll let you know when I'm on! Might even be earlier just depends when I get home from campus.
set — Yesterday at 10:45 PM
Sounds good, Ill be on all day. If I'm not responding feel free to use "@set" or whatever to notify me
eth — Yesterday at 10:49 PM
pretty much any day after 6 i'm available so just lmk when you need me!
set — Yesterday at 11:00 PM
@eth Sounds good, I'll see if I can get your part done tomorrow after I'm done with Lina, we dont have much to do left. I'll contact you if I do  tomorrow
lina6519 — 5:07 PM
about to hop on
I'm on
set — 5:12 PM
Perfect
Would you be able to hop on a call or do you want me to try to explain through text?
lina6519 — 5:12 PM
I can do a call!
set — 5:12 PM
Alright, are you availble right this moment?
lina6519 — 5:12 PM
Yes
set
 started a call. — 5:13 PM
set — 5:14 PM
I'm not able to hear you
<Window x:Class="FenceNote.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:viewmodels="clr-namespace:FenceNote.ViewModels"
        xmlns:mvvm="clr-namespace:FenceNote.MVVM"
        mc:Ignorable="d"
        Title="FenceNote" Height="650" Width="1000"
        WindowStartupLocation="CenterScreen"
        Background="{DynamicResource WindowBgBrush}"
        PreviewMouseDown="Window_PreviewMouseDown"
        PreviewKeyDown="Window_PreviewKeyDown">

    <Window.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis" />
        <SolidColorBrush x:Key="WindowBgBrush" Color="#F5F5F5" />
        <SolidColorBrush x:Key="Pane1BgBrush" Color="#DCDCDC" />
        <SolidColorBrush x:Key="Pane2BgBrush" Color="#EAEAEA" />
        <SolidColorBrush x:Key="Pane3BgBrush" Color="#FFFFFF" />
        <SolidColorBrush x:Key="TextPrimaryBrush" Color="#000000" />
        <SolidColorBrush x:Key="TextSecondaryBrush" Color="#888888" />
        <SolidColorBrush x:Key="BorderBrush" Color="#CCCCCC" />
        <SolidColorBrush x:Key="TextCaretBrush" Color="#000000" />
        <SolidColorBrush x:Key="HighlightBrush" Color="#0078D7" />
        <SolidColorBrush x:Key="Pane1HoverBrush" Color="#D0D0D0" />
        <SolidColorBrush x:Key="Pane2HoverBrush" Color="#E0E0E0" />
    </Window.Resources>

    <Window.InputBindings>
        <KeyBinding Key="T" Modifiers="Ctrl" Command="{Binding AddNoteCommand}" />
        <KeyBinding Key="V" Modifiers="Ctrl+Shift" Command="{Binding RequestCreateVaultCommand}" />
        <KeyBinding Key="W" Modifiers="Ctrl" Command="{Binding RequestDeleteNoteCommand}" />
    </Window.InputBindings>

    <Grid Background="Transparent">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="20" />
        </Grid.RowDefinitions>

        <Grid Grid.Row="0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200" />
                <ColumnDefinition Width="250" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <!-- Vaults Pane -->
            <Grid Grid.Column="0" Background="{DynamicResource Pane1BgBrush}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <Button Grid.Row="0" 
                        Content="PUBLIC NOTES" 
                        Command="{Binding ClearVaultSelectionCommand}" 
                        Margin="10,10,10,5" 
                        Padding="10" 
                        MinHeight="40"
                        FontWeight="Bold"
                        BorderThickness="0"
                        Cursor="Hand">
                    <Button.Style>
                        <Style TargetType="Button">
                            <Setter Property="Background" Value="{DynamicResource Pane2BgBrush}" />
                            <Setter Property="Foreground" Value="{DynamicResource HighlightBrush}" />
                            <Setter Property="Template">
                                <Setter.Value>
                                    <ControlTemplate TargetType="Button">
                                        <Border Background="{TemplateBinding Background}" Padding="{TemplateBinding Padding}" CornerRadius="4">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding IsPublicNotesSelected}" Value="True">
                                    <Setter Property="Background" Value="{DynamicResource HighlightBrush}" />
                                    <Setter Property="Foreground" Value="#EAEAEA" />
                                </DataTrigger>
                                <MultiDataTrigger>
                                    <MultiDataTrigger.Conditions>
                                        <Condition Binding="{Binding IsMouseOver, RelativeSource={RelativeSource Self}}" Value="True" />
                                        <Condition Binding="{Binding IsPublicNotesSelected}" Value="False" />
                                    </MultiDataTrigger.Conditions>
                                    <Setter Property="Background" Value="{DynamicResource Pane1HoverBrush}" />
                                </MultiDataTrigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>

                <TextBlock Grid.Row="1" 
                           Text="VAULTS" 
                           Foreground="{DynamicResource TextSecondaryBrush}" 
                           FontSize="11" ... (6 KB left)

MainWindow.xaml.txt
56 KB
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

MainWindow.xaml.cs.txt
9 KB
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

MainViewModel.cs.txt
27 KB
﻿
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

            Color pane1Hover = isDarkMode ? (Color)ColorConverter.ConvertFromString("#2D2D2E") : (Color)ColorConverter.ConvertFromString("#D0D0D0");
            Color pane2Hover = isDarkMode ? (Color)ColorConverter.ConvertFromString("#343437") : (Color)ColorConverter.ConvertFromString("#E0E0E0");

            AnimateColorResource("WindowBgBrush", windowBg);
            AnimateColorResource("Pane1BgBrush", pane1Bg);
            AnimateColorResource("Pane2BgBrush", pane2Bg);
            AnimateColorResource("Pane3BgBrush", pane3Bg);
            AnimateColorResource("TextPrimaryBrush", textPrimary);
            AnimateColorResource("TextSecondaryBrush", textSecondary);
            AnimateColorResource("BorderBrush", border);
            AnimateColorResource("TextCaretBrush", textCaret);
            AnimateColorResource("Pane1HoverBrush", pane1Hover);
            AnimateColorResource("Pane2HoverBrush", pane2Hover);
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
