using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;

namespace SpotifyXbox.Pages
{
    public sealed class TrackItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string PreviewUrl { get; set; }
        public string TrackId { get; set; }
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
            // Xbox: abre o browser do sistema com a pagina de login do Spotify
            await Launcher.LaunchUriAsync(new Uri(url));
            // nota: o redirect traz o code; num build completo usa-se um
            // AppService/protocol handler. Para o teste do Dev Mode, colar o
            // code manualmente numa caixa (simplificado).
            var codeBox = new ContentDialog
            {
                Title = "Cole o code do redirect",
                Content = new TextBox { HeaderText = "?code=..." },
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancelar"
            };
            var result = await codeBox.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var code = (codeBox.Content as TextBox).Text;
                if (await api.CompleteLoginAsync(code, loginVerifier))
                {
                    var me = await api.GetMeAsync();
                    UserText.Text = "Conectado: " + me.GetProperty("id").GetString();
                    await LoadTopTracksAsync();
                }
            }
        }

        private async System.Threading.Tasks.Task LoadTopTracksAsync()
        {
            // top tracks do usuario tem preview_url de 30s
            var playlists = await api.GetPlaylistsAsync();
            // simplificado: pega a primeira playlist e lista
            if (playlists.TryGetProperty("items", out var plist) &&
                plist.GetArrayLength() > 0)
            {
                var first = plist[0];
                var pid = first.GetProperty("id").GetString();
                var tracks = await api.GetPlaylistTracksAsync(pid);
                if (tracks.TryGetProperty("items", out var titems))
                {
                    foreach (var ti in titems.EnumerateArray())
                    {
                        var track = ti.GetProperty("track");
                        string preview = track.TryGetProperty("preview_url", out var p) &&
                                        p.ValueKind != System.Text.Json.JsonValueKind.Null
                            ? p.GetString() : null;
                        if (preview == null) continue;
                        items.Add(new TrackItem
                        {
                            Title = track.GetProperty("name").GetString(),
                            Artist = track.GetProperty("artists")[0].GetProperty("name").GetString(),
                            PreviewUrl = preview,
                            TrackId = track.GetProperty("id").GetString(),
                        });
                    }
                }
            }
        }

        private void OnTrackClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackItem t)
            {
                audio.PlayPreview(t.PreviewUrl, t.Title, t.Artist);
                NowPlaying.Text = t.Title;
                NowArtist.Text = t.Artist;
            }
        }

        private void OnPlayClick(object sender, RoutedEventArgs e) { audio.Pause(); }
        private void OnNextClick(object sender, RoutedEventArgs e) { audio.Next(); UpdateNowPlaying(); }
        private void OnPrevClick(object sender, RoutedEventArgs e) { audio.Prev(); UpdateNowPlaying(); }

        private void UpdateNowPlaying()
        {
            // reflecte o estado da fila no player bar
        }
    }
}
