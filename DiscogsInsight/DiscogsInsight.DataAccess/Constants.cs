namespace DiscogsInsight.DataAccess
{
    public static class Constants
    {
        public const string DatabaseFilename = "DiscogsInsight.db3";

        public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);


        //preferences hard coded strings should go here (and should only be used in settings data service)

        public static string LastFmUserName = "lastFmUsername";
        public static string LastFmPassword = "lastFmPassword";
        public static string LastFmApiKey = "lastFmApiKey";
        public static string DiscogsUsername = "discogsUsername";
    }
}
