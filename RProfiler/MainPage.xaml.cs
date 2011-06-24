using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;

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
            Uri url = new Uri("http://www.reddit.com/user/" + this.userName.Text + "/.xml", UriKind.Absolute);
            WebClient client = new WebClient();
            client.DownloadStringAsync(url);
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Client_DownloadStringCompleted);
        }

        void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            //Access the values from the event handler code.
            try
            {
                StringReader stream = new StringReader(e.Result);
                XmlReader reader = XmlReader.Create(stream);
                string test;
                if (!reader.IsEmptyElement)
                {
                    while (reader.Read())
                    {
                        if (reader.MoveToAttribute("item"))
                        {
                            switch (reader.Value.ToString())
                            {
                                case "endpoint":
                                    reader.MoveToElement();
                                    test = reader.ReadElementContentAsString();
                                    string hostUri = Application.Current.Host.Source.AbsoluteUri;

                                    //string targetUrl = EndPoint.Split('/')[2];

//                                    this.ApamEndpoint = "http://" + targetUrl + "/IdentityXAPAM/Auth";
//#if DEBUG
//                                    MessageBox.Show(this.ApamEndpoint.ToString());
//#endif
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unable to Find configuration Parameters");
                }
            }
            catch
            {
                MessageBox.Show("Configuration file not found or unable to access it.", "Config error", MessageBoxButton.OK);
            }
        }

        
    }
}
