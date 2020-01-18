using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Windows.Media.Imaging;


namespace FilmsDB
{
    public class Movie : INotifyPropertyChanged
    {
        private string title;
        private string poster;
        private string released;
        private string genre;
        private string director;
        private string actors;
        private string language;
        private string country;
        private string plot;
        private string runtime;
        private string note;
        private int rate_value;
        private bool watch_list_check;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Poster
        {
            get { return poster; }
            set
            {
                poster = value;
                OnPropertyChanged("Poster");
            }
        }

        public string Released
        {
            get { return released; }
            set
            {
                released = value;
                OnPropertyChanged("Released");
            }
        }
        public string Genre
        {
            get { return genre; }
            set
            {
                genre = value;
                OnPropertyChanged("Genre");
            }
        }
        public string Director
        {
            get { return director; }
            set
            {
                director = value;
                OnPropertyChanged("Director");
            }
        }
        public string Actors
        {
            get { return actors; }
            set
            {
                actors = value;
                OnPropertyChanged("Actors");
            }
        }
        public string Language
        {
            get { return language; }
            set
            {
                language = value;
                OnPropertyChanged("Language");
            }
        }
        public string Country
        {
            get { return country; }
            set
            {
                country = value;
                OnPropertyChanged("Country");
            }
        }
        public string Plot
        {
            get { return plot; }
            set
            {
                plot = value;
                OnPropertyChanged("Plot");
            }
        }
        public string Runtime
        {
            get { return runtime; }
            set
            {
                runtime = value;
                OnPropertyChanged("Runtime");
            }
        }

        public BitmapImage Image_path
        {
            get
            {
                string a = System.Environment.CurrentDirectory;
                if (poster != "N/A")
                {
                    string path = a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\img_cache.jpg";
                    BitmapImage image = new BitmapImage();
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(poster), path);
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri(path, UriKind.Absolute);
                        image.EndInit();
                    }
                    return image;
                }
                else
                {
                    string na_img = a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\NA_img.jpg";
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.UriSource = new Uri(na_img, UriKind.Absolute);
                    image.EndInit();
                    return image;
                }
            }
        }
        public string Note
        {
            get { return note; }
            set
            {
                note = value;
                OnPropertyChanged("Note");
            }
        }
        public int Rate_value
        {
            get { return rate_value; }
            set
            {
                rate_value = value;
                OnPropertyChanged("Rate_value1");
                OnPropertyChanged("Rate_value2");
                OnPropertyChanged("Rate_value3");
                OnPropertyChanged("Rate_value4");
                OnPropertyChanged("Rate_value5");
            }
        }
        public bool Rate_value1
        {
            get { return Rate_value.Equals(1); }
            set { Rate_value = 1; }
        }
        public bool Rate_value2
        {
            get { return Rate_value.Equals(2); }
            set { Rate_value = 2; }
        }
        public bool Rate_value3
        {
            get { return Rate_value.Equals(3); }
            set { Rate_value = 3; }
        }
        public bool Rate_value4
        {
            get { return Rate_value.Equals(4); }
            set { Rate_value = 4; }
        }
        public bool Rate_value5
        {
            get { return Rate_value.Equals(5); }
            set { Rate_value = 5; }
        }
        public bool Watch_list_check
        {
            get
            { return watch_list_check; }
            set
            {
                watch_list_check = value;
                OnPropertyChanged("Watch_list_check");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop) );
            }
        }
    }

}
