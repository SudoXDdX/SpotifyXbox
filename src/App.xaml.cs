using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SpotifyXbox
{
    /// <summary>
    /// Player Spotify pro Xbox Dev Mode.
    /// Arquitetura: Spotify Web API (OAuth PKCE) + Connect remoto +
    /// preview playback 30s com background audio via MediaSource + SMTC.
    /// </summary>
    public class App : Application
    {
        public static Frame RootFrame { get; private set; }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            RootFrame = new Frame();
            RootFrame.Navigate(typeof(Pages.MainPage), args.Arguments);
            Window.Current.Content = RootFrame;
            Window.Current.Activate();
        }
    }

    /// <summary>
    /// Task de background audio: registra pro sistema que o app toca midia
    /// e pode continuar rodando em background (Xbox suspende app sem audio).
    /// </summary>
    public sealed class BackgroundAudioTask
    {
        public void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            // o MediaPlayer + SMTC sao gerenciados pelo MainPage/AudioEngine;
            // esta task existe so' pra declarar o tipo audio no manifest.
            var deferral = taskInstance.GetDeferral();
            taskInstance.Task.Completed += (s, e) => deferral.Complete();
        }
    }
}
