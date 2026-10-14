using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyXbox.Pages
{
    /// <summary>
    /// Cliente da Spotify Web API com OAuth PKCE (device sem segredo).
    /// O app abre a pagina de login do Spotify no browser do Xbox,
    /// recebe o redirect com o code e troca pelo token.
    ///
    /// Quem for buildar precisa criar um Client ID em developer.spotify.com
    /// e colocar o redirect URI como https://sudo.local/callback (ou outro).
    /// </summary>
    public sealed class SpotifyApi
    {
        // PREENCHER: Client ID do dashboard do Spotify for Developers
        public const string ClientId = "SEU_CLIENT_ID_AQUI";
        public const string RedirectUri = "https://sudo.local/callback";
        public const string Scopes =
            "user-read-playback-state user-modify-playback-state " +
            "user-read-currently-playing streaming " +
            "playlist-read-private playlist-read-collaborative " +
            "user-library-read user-top-read";

        private string accessToken;
        private string refreshToken;
        private readonly HttpClient http = new();

        public bool IsLoggedIn => accessToken != null;

        // ---- OAuth PKCE ----
        public string BeginLogin(out string verifier)
        {
            var bytes = new byte[64];
            new Random().NextBytes(bytes);
            verifier = Convert.ToBase64String(bytes)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var challenge = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(verifier)))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var url = $"https://accounts.spotify.com/authorize?client_id={ClientId}" +
                      $"&response_type=code&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                      $"&scope={Uri.EscapeDataString(Scopes)}&code_challenge_method=S256" +
                      $"&code_challenge={challenge}";
            return url;
        }

        public async Task<bool> CompleteLoginAsync(string code, string verifier)
        {
            var body = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = RedirectUri,
                ["client_id"] = ClientId,
                ["code_verifier"] = verifier,
            };
            var resp = await http.PostAsync("https://accounts.spotify.com/api/token",
                new FormUrlEncodedContent(body));
            if (!resp.IsSuccessStatusCode) return false;
            var json = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
            accessToken = json.GetProperty("access_token").GetString();
            if (json.TryGetProperty("refresh_token", out var rt)) refreshToken = rt.GetString();
            return true;
        }

        private async Task<JsonElement> GetAsync(string path)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1" + path);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var resp = await http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        }

        public async Task<JsonElement> GetMeAsync() => await GetAsync("/me");
        public async Task<JsonElement> GetPlaylistsAsync() => await GetAsync("/me/playlists?limit=50");
        public async Task<JsonElement> GetPlaylistTracksAsync(string id) =>
            await GetAsync($"/playlists/{id}/tracks?limit=100");

        // ---- Connect remoto (o recurso principal pro Xbox) ----
        public async Task<JsonElement> GetDevicesAsync() => await GetAsync("/me/player/devices");
        public async Task<JsonElement> GetPlaybackStateAsync() => await GetAsync("/me/player");

        public async Task<bool> TransferPlaybackAsync(string deviceId)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { device_ids = new[] { deviceId }, play = true }),
                Encoding.UTF8, "application/json");
            return (await http.SendAsync(req)).IsSuccessStatusCode;
        }

        public async Task<bool> PlayPauseAsync(bool play)
        {
            var method = play ? HttpMethod.Put : HttpMethod.Put;
            var url = play ? "https://api.spotify.com/v1/me/player/play"
                            : "https://api.spotify.com/v1/me/player/pause";
            var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return (await http.SendAsync(req)).IsSuccessStatusCode;
        }

        public async Task<bool> NextTrackAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return (await http.SendAsync(req)).IsSuccessStatusCode;
        }

        public async Task<bool> PreviousTrackAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return (await http.SendAsync(req)).IsSuccessStatusCode;
        }

        public async Task<bool> SeekAsync(int positionMs)
        {
            var req = new HttpRequestMessage(HttpMethod.Put,
                $"https://api.spotify.com/v1/me/player/seek?position_ms={positionMs}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return (await http.SendAsync(req)).IsSuccessStatusCode;
        }
    }
}
