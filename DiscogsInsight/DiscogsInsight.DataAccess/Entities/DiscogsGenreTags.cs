﻿using DiscogsInsight.DataAccess.Contract;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class DiscogsGenreTags : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? DiscogsTag { get; set; }
    }
}
