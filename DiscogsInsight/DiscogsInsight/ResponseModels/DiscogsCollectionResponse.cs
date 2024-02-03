﻿namespace DiscogsInsight.ResponseModels
{
    using System;
    using System.Collections.Generic;


    public class ResponseArtist
    {
        public string anv { get; set; }
        public int id { get; set; }
        public string join { get; set; }
        public string name { get; set; }
        public string resource_url { get; set; }
        public string role { get; set; }
        public string tracks { get; set; }
    }

    public class BasicInformation
    {
        public List<ResponseArtist> artists { get; set; }
        public string cover_image { get; set; }
        public List<Format> formats { get; set; }
        public List<string> genres { get; set; }
        public int id { get; set; }
        public List<Label> labels { get; set; }
        public int master_id { get; set; }
        public string master_url { get; set; }
        public string resource_url { get; set; }
        public List<string> styles { get; set; }
        public string thumb { get; set; }
        public string title { get; set; }
        public int year { get; set; }
    }

    public class Format
    {
        public List<string> descriptions { get; set; }
        public string name { get; set; }
        public string qty { get; set; }
        public string text { get; set; }
    }

    public class Label
    {
        public string catno { get; set; }
        public string entity_type { get; set; }
        public string entity_type_name { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string resource_url { get; set; }
    }

    public class Pagination
    {
        public int items { get; set; }
        public int page { get; set; }
        public int pages { get; set; }
        public int per_page { get; set; }
        public Urls urls { get; set; }
    }

    public class Release
    {
        public BasicInformation basic_information { get; set; }
        public DateTime date_added { get; set; }
        public int id { get; set; }
        public int instance_id { get; set; }
        public int rating { get; set; }
    }

    public class DiscogsCollectionResponse
    {
        public Pagination pagination { get; set; }
        public List<Release> releases { get; set; }
    }

    public class Urls
    {
    }


}
