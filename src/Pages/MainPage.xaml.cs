using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Windows.ApplicationModel.Core;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace SpotifyXbox.Pages
{
    public sealed class TrackItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string PreviewUrl { get; set; }
        public string TrackId { get; set; }
    }

    /// <summary>
    /// Motor de audio: MediaPlayer + SMTC = Xbox reconhece como player
    /// e deixa tocando em background.
    /// </summary>
    public sealed class AudioEngine
    {
        private MediaPlayer player;

        public AudioEngine()
        {
            player = new MediaPlayer { AutoPlay = false };
            player.AudioCategory = MediaPlayerAudioCategory.Media;
        }

        public void PlayPreview(string url)
        {
            player.Source = MediaSource.CreateFromUri(new Uri(url));
            player.Play();
        }

        public void Pause() { player.Pause(); }
        public void Resume() { player.Play(); }
    }

    public sealed partial class MainPage : Page
    {
        private readonly SpotifyApi api = new();
        private readonly AudioEngine audio = new();
        private readonly ObservableCollection<TrackItem> items = new();
        private string loginVerifier;

        public MainPage()
        {
            this.InitializeComponent();
            TrackList.ItemsSource = items;
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var url = api.BeginLogin(out loginVerifier);
            await Launcher.LaunchUriAsync(new Uri(url));
        }

        private void OnTrackClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem t)
            {
                audio.PlayPreview(t.PreviewUrl);
                NowPlaying.Text = t.Title;
                NowArtist.Text = t.Artist;
            }
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            if (PlayButton.Content.ToString() == "▶") { audio.Resume(); PlayButton.Content = "⏸"; }
            else { audio.Pause(); PlayButton.Content = "▶"; }
        }

        private void OnNextClick(object sender, RoutedEventArgs e) { }
        private void OnPrevClick(object sender, RoutedEventArgs e) { }
    }
}
