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

namespace RProfiler
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

#if DEBUG
            this.userName.Text = "faintdeception";
#endif

            
        }

        

        private void ProfileMeButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a Uri with the address to the Yahoo Pipe.
            Uri url = new Uri(
              @"http://pipes.yahooapis.com/pipes/pipe.run?_id=2d136fd8e0a154f6bb996b216d590766&_render=json&userName=faintdeception");
            WebClient client = new WebClient();
            client.DownloadStringAsync(url);
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Client_DownloadStringCompleted);
        }

        void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            List<RedditEntry> entries = new List<RedditEntry>();

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

                foreach (RedditEntry entry in entries)
                {
                    //Get the uri of the post.
                    string uri = entry.Link;


                    //Split out the subreddit from the uri.
                    StringBuilder subredditParser = new StringBuilder(uri.ToString().Split('/')[4]);

                    subredditParser = subredditParser.Replace("\\", string.Empty).Replace("/", string.Empty);

                    string subreddit = subredditParser.ToString();

                    //this.subreddits.Add(subreddit);
                }

                //PopulateTagItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse JSON: " + ex.ToString());
            }
        }

        
    }
}
