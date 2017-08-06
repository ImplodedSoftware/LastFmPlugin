namespace LastFmPlugin.Helpers
{
    public class LastFmConstants
    {
public const string API_KEY = "<insert_key>";
public const string API_SECRET = "<insert_secret>";

        public const string URL_GET_ARTIST_INFO_BY_NAME =
            "http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={0}{1}&api_key=" + API_KEY + "&format=json";
        public const string URL_GET_ARTIST_INFO_BY_MBID =
            "http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={0}{1}&api_key=" + API_KEY + "&format=json";

        public const string URL_GET_SIMILAR_ARTIST_INFO_BY_NAME =
            "http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&artist={0}&api_key=" + API_KEY + "&format=json";
        public const string URL_GET_SIMILAR_ARTIST_INFO_BY_MBID =
            "http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&mbid={0}&api_key=" + API_KEY + "&format=json";

        public const string URL_GET_ALBUM_INFO_BY_NAME =
            "http://ws.audioscrobbler.com/2.0/?method=album.search&album={0}&api_key=" + API_KEY + "&format=json";

    }
}
