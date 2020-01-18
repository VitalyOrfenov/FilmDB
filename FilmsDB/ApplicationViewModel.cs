using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Data;




namespace FilmsDB
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        static string a = System.Environment.CurrentDirectory;
        public class RadioConverter : IValueConverter
        {
            public object Convert(object v, Type t, object p, System.Globalization.CultureInfo culture)
            {
                return v.Equals(p);
            }
            public object ConvertBack(object v, Type t, object p, System.Globalization.CultureInfo culture)
            {
                return System.Convert.ToBoolean(v) ? p : null;
            }
        }
        private string pattern;
        public string Pattern
        {
            get { return pattern; }
            set
            {
                Set(ref pattern, value);
                SelectedMovie = Movies.FirstOrDefault(s => s.Title.StartsWith(pattern));
            }
        }
        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Movie selectedMovie;
        public ObservableCollection<Movie> Watch_list { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }
        public Movie SelectedMovie
        {
            get {return selectedMovie;}
            set
            {
                selectedMovie = value;
                OnPropertyChanged("SelectedMovie");
            }
        }
        public ICommand AddMovieToList { get; set; }
        private void AddMovie(object parameter)
        {
            if (!(Watch_list.Contains(SelectedMovie)) && (SelectedMovie != null))
            {
                Watch_list.Add(SelectedMovie);
                SelectedMovie.Watch_list_check = true;
                List<string> csv_string = new List<string> { "title|user_note|user_rate|user_watch_list_flag" };
                using (var writer = new StreamWriter(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\user_cache.csv"))
                {
                    foreach (Movie mov in Watch_list)
                    {
                        string str = String.Format("{0}|{1}|{2}|{3}", mov.Title, mov.Note, mov.Rate_value, mov.Watch_list_check);
                        writer.WriteLine(str);
                    }
                }
            }
        }
        public ApplicationViewModel()
        {
            AddMovieToList = new RelayCommand(AddMovie);
            Movies = new ObservableCollection<Movie> { };
            Watch_list = new ObservableCollection<Movie> { };
            List<string> csv_string = new List<string> { "title|released|plot|director|actors|language|country|poster|runtime|genre|user_note|user_rate|user_watch_list_flag" };
            if (!(File.Exists(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\cache.csv")))
            {
                var connection = DBComms.DBConnection("localhost", "5432", "postgre", "28081998", "kino");
                var movie_list = DBComms.GetMoviesInfo(connection);
                while (movie_list.Read())
                {
                    Movie movie = new Movie
                    {
                        Title = movie_list.GetString(0),
                        Poster = movie_list.GetString(7),
                        Released = Convert.ToString(movie_list.GetDate(1)),
                        Director = movie_list.GetString(3),
                        Actors = movie_list.GetString(4),
                        Language = movie_list.GetString(5),
                        Country = movie_list.GetString(6),
                        Plot = movie_list.GetString(2),
                        Runtime = Convert.ToString(movie_list.GetInt32(8)) + " min",
                        Genre = movie_list.GetString(9),
                        Note = null,
                        Rate_value = 0,
                        Watch_list_check = false
                        
                    };
                    Movies.Add(movie);
                    string row = String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}", movie.Title, movie.Released, movie.Plot,
                                                movie.Director, movie.Actors, movie.Language, movie.Country, movie.Poster, 
                                                movie.Runtime, movie.Genre, movie.Note, movie.Rate_value, movie.Watch_list_check);
                    csv_string.Add(row);
                }
                using (var writer = new StreamWriter(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\cache.csv"))
                {
                    foreach (string str in csv_string)
                    {
                        writer.WriteLine(str);
                    }
                }
                DBComms.DBDisconnect(connection);
            }
            else
            {
                using (var reader = new StreamReader(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\cache.csv"))
                {
                    string rows = reader.ReadToEnd();
                    foreach (string row in rows.Split('\n'))
                    {
                        if (!(row.Contains("title|released")))
                        {
                            string[] movie_data = row.Split('|');
                            if (movie_data[0].Length != 0)
                            {
                                Movie movie = new Movie
                                {
                                    Title = movie_data[0],
                                    Poster = movie_data[7],
                                    Released = movie_data[1],
                                    Director = movie_data[3],
                                    Actors = movie_data[4],
                                    Language = movie_data[5],
                                    Country = movie_data[6],
                                    Plot = movie_data[2],
                                    Runtime = movie_data[8],
                                    Genre = movie_data[9],
                                    Note = movie_data[10],
                                    Rate_value = Convert.ToInt32(movie_data[11]),
                                    Watch_list_check = Convert.ToBoolean(movie_data[12])
                                };
                                if ((File.Exists(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\user_cache.csv")))
                                {
                                    using (var user_reader = new StreamReader(a.Substring(0, a.IndexOf("\\bin")) + "\\cache\\user_cache.csv"))
                                    {
                                        string[] user_rows = user_reader.ReadToEnd().Split('\n');
                                        foreach (string user_row in user_rows)
                                        {
                                            string[] user_movie_data = user_row.Split('|');
                                            if (movie.Title == user_movie_data[0])
                                            {
                                                movie.Note = user_movie_data[1];
                                                movie.Rate_value = Convert.ToInt32(user_movie_data[2]);
                                                movie.Watch_list_check = Convert.ToBoolean(user_movie_data[3]);
                                            }
                                        }
                                    }
                                }
                                Movies.Add(movie);
                                if (movie.Watch_list_check)
                                {
                                    Watch_list.Add(movie);
                                }
                            }
                        }
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
