namespace LastFmPlugin.Types
{
    public static class LastFmHelper
    {
        public static int LastFmSizeToInt(string size)
        {
            switch (size.ToLower())
            {
                case "":
                    return 0;
                case "small":
                    return 1;
                case "medium":
                    return 2;
                case "large":
                    return 3;
                case "extralarge":
                    return 4;
                case "mega":
                    return 5;
            }
            return 0;
        }
    }
    public class LastFmImageHelper
    {
        public string Url { get; set; }
        public int Size { get; set; }
    }
}
