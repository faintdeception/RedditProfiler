using System.Runtime.Serialization;
using System.ComponentModel;

namespace RProfiler
{
  [DataContract]
  public class RedditEntry
  {
    /// <summary>
    /// Gets or sets the title of the reddit post.
    /// </summary>
    [DataMember(Name="title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image.
    /// </summary>
    //[DataMember(Name="thumbnail")]
    //public string Thumbnail { get; set; }

    
    /// <summary>
    /// Gets or sets the url to actual contents.
    /// </summary>
    [DataMember(Name="link")]
    public string Link { get; set; }

    /// <summary>
    /// Gets or sets the number of comments.
    /// </summary>
    //[DataMember(Name="num_comments")]
    //public int NumComments { get; set; }
  }
}
