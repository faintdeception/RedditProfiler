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
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RProfiler
{
    public partial class MainPage : UserControl
    {

        //List<RedditEntry> _entries = new List<RedditEntry>();


        #region _entries (DependencyProperty)

        /// <summary>
        /// The list of all comments
        /// </summary>
        public ObservableCollection<RedditEntry> _entries
        {
            get { return (ObservableCollection<RedditEntry>)GetValue(_entriesProperty); }
            set { SetValue(_entriesProperty, value); }
        }
        public static readonly DependencyProperty _entriesProperty =
            DependencyProperty.Register("_entries", typeof(ObservableCollection<RedditEntry>), typeof(MainPage),
            new PropertyMetadata(null, new PropertyChangedCallback(On_entriesChanged)));

        private static void On_entriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainPage)d).On_entriesChanged(e);
        }

        protected virtual void On_entriesChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion //_entries


        string _lastIdPulled;
        bool _resetPushed;
        bool _isDownloading;
        WebClient _client = new WebClient();

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

        #region CommentCount (DependencyProperty)

        /// <summary>
        /// The total number of comments scanned on the current search.
        /// </summary>
        public int CommentCount
        {
            get { return (int)GetValue(CommentCountProperty); }
            set { SetValue(CommentCountProperty, value); }
        }
        public static readonly DependencyProperty CommentCountProperty =
            DependencyProperty.Register("CommentCount", typeof(int), typeof(MainPage),
            new PropertyMetadata(0, new PropertyChangedCallback(OnCommentCountChanged)));

        private static void OnCommentCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainPage)d).OnCommentCountChanged(e);
        }

        protected virtual void OnCommentCountChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion //CommentCount


        #region CurrentDepth (DependencyProperty)

        /// <summary>
        /// How many levels deep are you on the current profile.
        /// </summary>
        public int CurrentDepth
        {
            get { return (int)GetValue(CurrentDepthProperty); }
            set { SetValue(CurrentDepthProperty, value); }
        }
        public static readonly DependencyProperty CurrentDepthProperty =
            DependencyProperty.Register("CurrentDepth", typeof(int), typeof(MainPage),
            new PropertyMetadata(0, new PropertyChangedCallback(OnCurrentDepthChanged)));

        private static void OnCurrentDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainPage)d).OnCurrentDepthChanged(e);
        }

        protected virtual void OnCurrentDepthChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion //CurrentDepth


        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Subreddits = new ObservableCollection<WeightedSubreddit>();
            this._entries = new ObservableCollection<RedditEntry>();

            this.DetailedList.DataContext = this.SubredditList.SelectedItem;

#if DEBUG
            this.userName.Text = "faintdeception";
#endif

        }


        private void ProfileMeButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentDepth = 1;

            this._isDownloading = true;

            this.EnsureVisualState();
            // Create a Uri with the address to the Yahoo Pipe.
            Uri uri = new Uri(
              @"http://pipes.yahooapis.com/pipes/pipe.run?_id=2d136fd8e0a154f6bb996b216d590766&_render=json&userName=" + this.userName.Text);
            this.GetComments(uri);
        }

        void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                this._isDownloading = false;
                this.EnsureVisualState();
                return;
            }

            // Initialize the deserializer.
            DataContractJsonSerializer jsonSerializer =
              new DataContractJsonSerializer(typeof(RedditEntry));

            try
            {
                JsonTextReader reader = new JsonTextReader(new
                StringReader(e.Result));
                // Dig through the JSON and find the array of results.
                // The tree is: response -> value -> items -> [0] -> data -> children[]
                JObject jsonResponse = (JObject)JObject.Load(reader);
                JObject jsonValue = (JObject)jsonResponse["value"];
                JObject jsonItems = (JObject)((JArray)jsonValue["items"])[0];
                JObject jsonData = (JObject)jsonItems["data"];

                JArray jsonChildren = jsonData != null ? (JArray)jsonData["children"] : null;

                if (jsonChildren == null)
                {
                    this._resetPushed = true;
                    throw new Exception("Unable to load comments.");
                }

                //Pull the last id off the the returned comments and compare it with the last id we got.
                //I'm assuming if they are equal that we just got the same last page over again.
                //If there is no last id then that also indicates we're on the last page.

                if (_lastIdPulled != null && _lastIdPulled == ((Newtonsoft.Json.Linq.JValue)(jsonData["after"])).Value.ToString())
                {
                    MessageBox.Show("We can't go any deeper!");
                    return;
                }


                _lastIdPulled = ((Newtonsoft.Json.Linq.JValue)(jsonData["after"])).Type == JTokenType.Null ? null : ((Newtonsoft.Json.Linq.JValue)(jsonData["after"])).Value.ToString();

                // Iterate through every child.
                foreach (JObject child in jsonChildren)
                {
                    // Get the data for the reddit post.
                    JObject data = (JObject)child["data"];

                    // Create a memory stream of the JSON text
                    // to be passed to the deserializer.
                    using (MemoryStream memStream =
                      new MemoryStream(UTF8Encoding.UTF8.GetBytes(data.ToString())))
                    {
                        // Add the entry to the collection.
                        _entries.Add((RedditEntry)jsonSerializer.ReadObject(memStream));
                    }
                }

                Dictionary<string, List<RedditEntry>> subredditWeigher = new Dictionary<string, List<RedditEntry>>();

                //cloud.ItemsSource = _entries.Select(d => d.Subreddit);
                foreach (RedditEntry entry in _entries)
                {
                    string subreddit = entry.Subreddit;

                    if (subredditWeigher.ContainsKey(subreddit))
                        subredditWeigher[subreddit].Add(entry);
                    else
                    {
                        subredditWeigher.Add(subreddit, new List<RedditEntry>());
                        subredditWeigher[subreddit].Add(entry);
                    }
                }

                this.Subreddits.Clear();
                foreach (var item in subredditWeigher)
                {
                    this.Subreddits.Add(new WeightedSubreddit() { Name = item.Key, Weight = (item.Value as List<RedditEntry>).Count, Entries = item.Value as List<RedditEntry>});
                }

                this.Subreddits.BubbleSort();
                //this.SubredditList.ItemsSource = this.Subreddits;





                //PopulateTagItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse JSON: " + ex.Message.ToString());
            }
            finally
            {
                this._isDownloading = false;

                this.EnsureVisualState();
            }
        }

        private void GoDeeperButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Need to send the id of the last entry along with the query.

            this.CurrentDepth++;

            this._isDownloading = true;

            this.EnsureVisualState();

            if (_lastIdPulled == null || _lastIdPulled.Length == 0)
            {
                MessageBox.Show("We cannot go any deeper");
                this._isDownloading = false;

                this.EnsureVisualState();
                return;
            }


            // Create a Uri with the address to the Yahoo Pipe.
            Uri uri = new Uri(
              @"http://pipes.yahooapis.com/pipes/pipe.run?_id=2d136fd8e0a154f6bb996b216d590766&_render=json&afterId=" + _lastIdPulled + "&userName=" + this.userName.Text);

            this.GetComments(uri);
        }

        private void GetComments(Uri uri)
        {
            _client = new WebClient();
            _client.DownloadStringAsync(uri);
            _client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Client_DownloadStringCompleted);

        }

        private void EnsureVisualState()
        {
            //If no user data is found then its probably a new user, go to new user screen.
            if (this._resetPushed)
                VisualStateManager.GoToState(this, "Outside", true);
            else
                VisualStateManager.GoToState(this, "Inside", true);

            if (this._isDownloading)
                VisualStateManager.GoToState(this, "Downloading", true);
            else
                VisualStateManager.GoToState(this, "Downloaded", true);

            this._resetPushed = false;
        }

        private void GetOut_ButtonClick(object sender, RoutedEventArgs e)
        {
            this.CurrentDepth = 0;

            this._client.CancelAsync();

            this._entries.Clear();
            this.Subreddits.Clear();

            this._resetPushed = true;

            this.EnsureVisualState();
        }

        private void userName_GotFocus(object sender, RoutedEventArgs e)
        {
            this.userName.SelectAll();
        }

        private void SubredditList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DetailedList.DataContext = (WeightedSubreddit)SubredditList.SelectedItem;
            this.DetailedList.ItemsSource = ((WeightedSubreddit)SubredditList.SelectedItem).Entries;
        }


    }

    public class WeightedSubreddit : IComparable
    {
        private List<RedditEntry> _redditEntries;
        public string Name { get; set; }
        public int Weight { get; set; }
        public List<RedditEntry> Entries
        {
            get
            {
                if (this._redditEntries == null)
                    this._redditEntries =  new List<RedditEntry>();

                return this._redditEntries;
            }

            set
            {
                this._redditEntries = value;
            }
        }

        public int CompareTo(object obj)
        {
            return (obj as WeightedSubreddit).Weight.CompareTo(this.Weight);
        }
    }

    public static class ListExtension
    {
        public static void BubbleSort(this IList o)
        {
            for (int i = o.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    object o1 = o[j - 1];
                    object o2 = o[j];
                    if (((IComparable)o1).CompareTo(o2) > 0)
                    {
                        o.Remove(o1);
                        o.Insert(j, o1);
                    }
                }
            }
        }
    }
}