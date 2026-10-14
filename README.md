# Spotify Xbox Dev Mode (SudoXD Edition)

Player Spotify pra Xbox em Dev Mode: OAuth PKCE, Spotify Connect remoto e
previews de 30s com SMTC (background audio de verdade).

## Estado honesto do build

O source tá completo, mas compilar UWP/XAML exige Windows: o XamlCompiler do
WindowsAppSDK é um executável .NET Framework (net472) Windows-only, e a
projection do SDK.NET não expõe os types do Windows.UI.Xaml pra código
compilado fora do Windows. Testado a fundo no Termux: sem jeito de compilar
aqui.

## Como buildar (Windows, 5 minutos)

Requisitos: Windows 10/11 com .NET 10 SDK e o workload UWP.

```powershell
# no PC Windows
git clone <este repo>
cd src
dotnet restore
dotnet build -p:Platform=x64 -p:Configuration=Release

# empacotar APPX
makeappx pack /d bin\x64\Release\net10.0-windows10.0.22621.0\win-x64 /p ..\SpotifyXbox.appx

# assinar (Dev Mode aceita cert de dev)
signtool sign /fd SHA256 /a /f cert.pfx SpotifyXbox.appx
```

Depois: Xbox Dev Mode > Add packages > seleciona o APPX.

## Antes do primeiro build

1. Cria um app em https://developer.spotify.com/dashboard
2. Copia o Client ID pro `SpotifyApi.cs` (SEU_CLIENT_ID_AQUI)
3. Adiciona o redirect URI `https://sudo.local/callback` no dashboard
4. Builda

## O que tem no source

- `SpotifyApi.cs`: cliente completo da Web API com OAuth PKCE, playlists,
  library, e o Spotify Connect (transfer de playback, play/pause/next/seek)
- `AudioEngine` (MainPage.xaml.cs): MediaPlayer + SystemMediaTransportControls,
  que é o que faz o Xbox reconhecer como player e deixar em background
- `MainPage.xaml`: UI verde Spotify com lista de faixas e player bar
- `Package.appxmanifest`: TargetDeviceFamily Windows.Xbox, background audio
  declarado, splash screen e tiles verdes

## Sobre o Spicetify

Spicetify é injeção no client desktop (Electron/CEF) — não existe client
Spotify desktop no Xbox, então não há o que injetar. O papel visual do
Spicetify aqui é o tema próprio verde, e o papel funcional (features que o
Spotify some) vira o Connect remoto + previews.
