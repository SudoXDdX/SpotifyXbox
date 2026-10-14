using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SpotifyXbox
{
    /// <summary>
    /// Player Spotify pro Xbox Dev Mode (SudoXD Edition).
    /// Web API + Connect remoto + preview 30s com SMTC (background audio).
    /// </summary>
    public partial class App : Application
    {
        public static Frame RootFrame { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            RootFrame = new Frame();
            RootFrame.Navigate(typeof(Pages.MainPage), args.Arguments);
            m_window = new Microsoft.UI.Xaml.Window();
            m_window.Content = RootFrame;
            m_window.Activate();
        }

        private Microsoft.UI.Xaml.Window m_window;
    }
}
