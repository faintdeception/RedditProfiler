using System.Runtime.Serialization;
using System.ComponentModel;
using System;

namespace RProfiler
{
    [DataContract]
    public class RedditEntry
    {
        /// <summary>
        /// Gets or sets the title of the reddit post.
        /// </summary>
        [DataMember(Name = "link_title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the plaintext contents of the comment.
        /// </summary>
        [DataMember(Name = "body")]
        public string Comment { get; set; }

        /// <summary>
        /// Gets/sets the subreddit name of this entry.
        /// </summary>
        [DataMember(Name = "subreddit")]
        public string Subreddit { get; set; }

        /// <summary>
        /// Gets or sets the name of the post.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the id of the comment.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the parent id of the comment.
        /// </summary>
        [DataMember(Name = "parent_id")]
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the subreddit id of the comment.
        /// </summary>
        [DataMember(Name = "subreddit_id")]
        public string SubredditId { get; set; }

        /// <summary>
        /// Gets or sets the link id of the comment.
        /// </summary>
        [DataMember(Name = "link_id")]
        public string LinkId { get; set; }

        public Uri ResolvedUri
        {
            get
            {
                string link_id = (LinkId.Length > 0 && LinkId.Contains("_")) ? LinkId.Split('_')[1] : "";
                string parent_id = (ParentId.Length > 0 && ParentId.Contains("_")) ? ParentId.Split('_')[1] : "";


                string parsedUri = "http://www.reddit.com/r/" + Subreddit + "/comments/" + link_id + "/" + parent_id + "/" + Id;

                return new Uri(parsedUri, UriKind.RelativeOrAbsolute);

            }
        }
    }
}
