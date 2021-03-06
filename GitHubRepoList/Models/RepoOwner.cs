﻿using System;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Models
{
    [Serializable]
    public class RepoOwner
    {
        public int id { get; set; }

        public string login { get; set; }

        public string avatar_url { get; set; }

        public string url { get; set; }

        public string repos_url { get; set; }

        public string type { get; set; }

        // public string userpic { get; set; }

        [ScriptIgnore]
        public byte[] binary_userpic { get; set; }
    }
}