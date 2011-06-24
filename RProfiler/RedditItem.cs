using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RProfiler
{
    public class RedditItem
    {
        public class RedditEntry
        {
            /// <summary>
            /// Gets or sets the GUID of the reddit post.
            /// </summary>
            public string PostUniqueId { get; set; }

            /// <summary>
            /// Gets or sets the title of the reddit post.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the url to the Reddit comments page.
            /// </summary>
            public string Permalink { get; set; }

            /// <summary>
            /// Gets or sets the description of the reddit post.
            /// </summary>
            public string PostDescription { get; set; }
        }
    }
}
