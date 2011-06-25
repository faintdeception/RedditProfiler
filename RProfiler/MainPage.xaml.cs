using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Browser;
using System.Collections.ObjectModel;

namespace RProfiler
{
    public partial class MainPage : UserControl
    {


        #region Subreddits (DependencyProperty)

        /// <summary>
        /// The collection of subreddits
        /// </summary>
        public ObservableCollection<WeightedSubreddit> Subreddits
        {
            get { return (ObservableCollection<WeightedSubreddit>)GetValue(SubredditsProperty); }
            set { SetValue(SubredditsProperty, value); }
        }
        public static readonly DependencyProperty SubredditsProperty =
            DependencyProperty.Register("Subreddits", typeof(ObservableCollection<WeightedSubreddit>), typeof(MainPage),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSubredditsChanged)));

        private static void OnSubredditsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainPage)d).OnSubredditsChanged(e);
        }

        protected virtual void OnSubredditsChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion //Subreddits
        



                
        
        
        public MainPage()
        {
            InitializeComponent();

#if DEBUG
            this.userName.Text = "faintdeception";
#endif

            this.Subreddits = new ObservableCollection<WeightedSubreddit>();


        }

        

        private void ProfileMeButton_Click(object sender, RoutedEventArgs e)
        {   
            // Create a Uri with the address to the Yahoo Pipe.
            Uri url = new Uri(
              @"http://pipes.yahooapis.com/pipes/pipe.run?_id=2d136fd8e0a154f6bb996b216d590766&_render=json&userName=" + this.userName.Text);
            WebClient client = new WebClient();
            client.DownloadStringAsync(url);
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Client_DownloadStringCompleted);
        }

        void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            List<RedditEntry> entries = new List<RedditEntry>();
            this.Subreddits.Clear();
            // Initialize the deserializer.
            DataContractJsonSerializer jsonSerializer =
              new DataContractJsonSerializer(typeof(RedditEntry));

            try
            {
                // Dig through the JSON and find the array of results.
                // The tree is: response -> value -> items -> [0] -> data -> children[]
                JsonObject jsonResponse = (JsonObject)JsonObject.Load(new StringReader(e.Result));
                JsonObject jsonValue = (JsonObject)jsonResponse["value"];
                JsonObject jsonItems = (JsonObject)((JsonArray)jsonValue["items"])[0];
                JsonObject jsonData = (JsonObject)jsonItems["channel"];
                JsonArray jsonChildren = (JsonArray)jsonData["item"];

                // Iterate through every child.
                foreach (JsonObject child in jsonChildren)
                {

                    // Create a memory stream of the JSON text
                    // to be passed to the deserializer.
                    using (MemoryStream memStream =
                      new MemoryStream(UTF8Encoding.UTF8.GetBytes(child.ToString())))
                    {
                        // Add the entry to the collection.
                        entries.Add((RedditEntry)jsonSerializer.ReadObject(memStream));
                    }


                }

                Dictionary<string, int> subredditWeigher = new Dictionary<string, int>();


                foreach (RedditEntry entry in entries)
                {
                    //Get the uri of the post.
                    string uri = entry.Link;


                    //Split out the subreddit from the uri.
                    StringBuilder subredditParser = new StringBuilder(uri.ToString().Split('/')[4]);

                    subredditParser = subredditParser.Replace("\\", string.Empty).Replace("/", string.Empty);

                    string subreddit = subredditParser.ToString();

                    if (subredditWeigher.ContainsKey(subreddit))
                        subredditWeigher[subreddit]++;
                    else
                        subredditWeigher.Add(subreddit,1);                    
                }


                foreach (var item in subredditWeigher)
                {
                    this.Subreddits.Add(new WeightedSubreddit() { Name = item.Key, Weight = item.Value });
                }

                this.SubredditList.ItemsSource = this.Subreddits;
                
                

                //PopulateTagItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse JSON: " + ex.ToString());
            }
        }

        
    }

    public class WeightedSubreddit
    {
        public string Name { get; set; }
        public int Weight { get; set; }
    }
}
