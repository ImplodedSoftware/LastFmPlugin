using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ImPluginEngine.Interfaces;
using LastFmPlugin.Helpers;
using System.Net.Http;
using System.Threading;
using System.Windows.Media.Imaging;
using ImPluginEngine.Abstractions.Interfaces;
using ImPluginEngine.Types;
using Newtonsoft.Json.Linq;
using ImPluginEngine.Helpers;
using LastFmPlugin.Types;

namespace LastFmPlugin
{
    public class LastFmPlugin : IPlugin, IArtistPlugin, IArtistPicture, IAlbumPicture
    {
        public string Name => "Last.fm";

        public string Version => "1.0";


        public async Task<PluginArtist> GetArtistData(PluginArtist artist, CancellationToken ct)
        {
            var url = string.Format(LastFmConstants.URL_GET_ARTIST_INFO_BY_NAME, HttpUtility.UrlEncode(artist.Name));
            if (!string.IsNullOrEmpty(artist.MusicBrainzId))
                url = string.Format(LastFmConstants.URL_GET_ARTIST_INFO_BY_MBID, artist.MusicBrainzId);
            var client = new HttpClient();
            string json;
            try
            {
                var resp = await client.GetAsync(url, ct);
                var data = await resp.Content.ReadAsByteArrayAsync();
                json = Encoding.UTF8.GetString(data);
            }
            catch (HttpRequestException)
            {
                return new PluginArtist();
            }

            var res = new PluginArtist();

            var jObject = JObject.Parse(json);

            var jArtist = jObject["artist"];
            if (jArtist == null)
                return null;

            var name = JsonHelper.GetStringOrDefault(jArtist["name"]);

            var imageList = new List<LastFmImageHelper>();
            var jImages = jArtist["image"];
            foreach (var image in jImages)
            {
                var io = new LastFmImageHelper {Url = JsonHelper.GetStringOrDefault(image["#text"])};
                var sizeString = JsonHelper.GetStringOrDefault(image["size"]);
                io.Size = LastFmHelper.LastFmSizeToInt(sizeString);
                imageList.Add(io);
            }
            imageList = imageList.OrderByDescending(x => x.Size).ToList();
            var jBio = jArtist["bio"];
            var bio = JsonHelper.GetStringOrDefault(jBio["content"]);

            res.Name = name;
            res.Biography = bio;
            var bestImage = imageList.First();
            if (bestImage != null && !string.IsNullOrEmpty(bestImage.Url))
            {
                var imageUrl = bestImage.Url;
                var uid = Guid.NewGuid().ToString();
                var ext = imageUrl.Substring(imageUrl.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase));
                uid = uid + ext;
                var pFile = Path.Combine(PluginConstants.TempPath, uid);
                File.WriteAllBytes(pFile, new WebClient().DownloadData(imageUrl));
                res.ImageName = pFile;
            }

            // similar
            url = string.Format(LastFmConstants.URL_GET_SIMILAR_ARTIST_INFO_BY_NAME, HttpUtility.UrlEncode(artist.Name));
            if (!string.IsNullOrEmpty(artist.MusicBrainzId))
                url = string.Format(LastFmConstants.URL_GET_SIMILAR_ARTIST_INFO_BY_MBID, artist.MusicBrainzId);
            client = new HttpClient();
            try
            {
                json = await client.GetStringAsync(url);

                jObject = JObject.Parse(json);

                var similarRoot = jObject["similarartists"];
                var similarArtists = similarRoot["artist"];
                res.SimilarArtists = new List<string>();
                foreach (var sa in similarArtists)
                {
                    res.SimilarArtists.Add(JsonHelper.GetStringOrDefault(sa["name"]));
                }
            }
            catch
            { 
                // tolerate errors 
            }

            return res;
        }

        public async Task GetArtistPicture(PluginArtist artist, CancellationToken ct, IPluginCallback updateCallback, Action<int, string> updateAction)
        {
            var artistData = await GetArtistData(artist, ct);
            var res = new PluginImage();
            if (!string.IsNullOrEmpty(artistData?.ImageName))
            {
                res.Filename = artistData.ImageName;
                using (var stream = new FileStream(res.Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation,
                        BitmapCacheOption.None);
                    res.Width = bitmapFrame.PixelWidth;
                    res.Height = bitmapFrame.PixelHeight;
                }
                res.FoundByPlugin = Name;
            }
            if (updateAction != null)
                updateAction(artist.Id, res.Filename);
            else
                updateCallback.PictureUpdateNotification(res);
        }

        public async Task GetAlbumPicture(PluginAlbum album, CancellationToken ct, IPluginCallback updateCallback, Action<int, string> updateAction)
        {
            var url = string.Format(LastFmConstants.URL_GET_ALBUM_INFO_BY_NAME, HttpUtility.UrlEncode(album.AlbumName));
            var client = new HttpClient();
            var json = string.Empty;
            try
            {
                var resp = await client.GetAsync(url, ct);
                var data = await resp.Content.ReadAsByteArrayAsync();
                json = Encoding.Default.GetString(data);
            }
            catch (HttpRequestException)
            {
                return;
            }

            var res = new PluginImage();

            var imageName = string.Empty;
            var jObject = JObject.Parse(json);
            var ar = jObject["results"]["albummatches"]["album"];
            foreach (var item in ar.Children())
            {
                var artist = JsonHelper.GetStringOrDefault(item["artist"]);
                var albumname = JsonHelper.GetStringOrDefault(item["name"]);
                if (artist.Equals(album.AlbumArtist, StringComparison.InvariantCultureIgnoreCase))
                {
                    var imageList = new List<LastFmImageHelper>();
                    var jImages = item["image"];
                    foreach (var image in jImages)
                    {
                        var io = new LastFmImageHelper {Url = JsonHelper.GetStringOrDefault(image["#text"])};
                        var sizeString = JsonHelper.GetStringOrDefault(image["size"]);
                        io.Size = LastFmHelper.LastFmSizeToInt(sizeString);
                        imageList.Add(io);
                    }
                    imageList = imageList.OrderByDescending(x => x.Size).ToList();
                    var bestImage = imageList.First();
                    if (bestImage != null && !string.IsNullOrEmpty(bestImage.Url))
                    {
                        var imageUrl = bestImage.Url;
                        var uid = Guid.NewGuid().ToString();
                        var ext = imageUrl.Substring(imageUrl.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase));
                        uid = uid + ext;
                        var pFile = Path.Combine(PluginConstants.TempPath, uid);
                        File.WriteAllBytes(pFile, new WebClient().DownloadData(imageUrl));
                        imageName = pFile;
                        break;
                    }

                }
            }
            
            if (!string.IsNullOrEmpty(imageName))
            {
                res.Filename = imageName;
                using (var stream = new FileStream(res.Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation,
                        BitmapCacheOption.None);
                    res.Width = bitmapFrame.PixelWidth;
                    res.Height = bitmapFrame.PixelHeight;
                }
                res.Id = album.Id;
                res.FoundByPlugin = Name;
            }
            if (updateAction == null)
            {
                updateCallback.PictureUpdateNotification(res);
            }
            else
            {
                updateAction(album.Id, imageName);
            }
        }
    }
}
