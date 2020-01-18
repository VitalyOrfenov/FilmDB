using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Npgsql;

namespace FilmsDB
{
    class DBComms
    {
        public static NpgsqlConnection DBConnection(string serv_adress, string port, string user_id, string pass, string db_name)
        //Соединение с базой данных
        {
            Npgsql.NpgsqlConnection connect = new Npgsql.NpgsqlConnection(String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4}", serv_adress, port, user_id, pass, db_name));
            connect.Open();
            return connect;
        }

        public static void DBDisconnect(NpgsqlConnection connect)
        {
            connect.Close();
        }

        public static int DBRowCounter(string table, NpgsqlConnection conn)
        //Счет количества строк в таблице базы данных
        {
            string rowcounter = String.Format("SELECT COUNT(*) FROM {0}", table);
            NpgsqlConnection connection = conn;
            NpgsqlCommand cnt = new NpgsqlCommand(rowcounter, connection);
            int id_counter = Convert.ToInt32(cnt.ExecuteScalar());
            return id_counter;
        }

        public static List<Movie> ReadMovieJSON(string filename)
        //Чтение JSON и создание списка кино
        {
            List<Movie> movie_list = new List<Movie>();
            using (StreamReader file = File.OpenText(filename))
            {
                string line = "";
                while ((line = file.ReadLine()) != null)
                {
                    Movie m = JsonConvert.DeserializeObject<Movie>(line);
                    if (!((m.Released == "N/A") || (m.Runtime == "N/A")))
                    {
                        movie_list.Add(m);
                    }
                }
            }
            return movie_list;
        }

        public static void UploadMovieData(Movie movie, int id_counter, NpgsqlConnection connect)
        //Вставка данных об одном фильме в таблицу kino
        {
            string insert_kino = "INSERT INTO kino (id, title, release_date, " +
                                        "plot, director, actors , language, country," +
                                        " poster_uri, runtime) VALUES (@id, @title, " +
                                        "CAST(@release_date AS date), @plot, " +
                                        "@director, @actors, @language, @country," +
                                        " @poster_uri, @runtime)";
            using (NpgsqlCommand cmd = new NpgsqlCommand(insert_kino, connect))
            {
                cmd.Parameters.AddWithValue("id", id_counter);
                cmd.Parameters.AddWithValue("title", movie.Title);
                cmd.Parameters.AddWithValue("release_date", movie.Released);
                cmd.Parameters.AddWithValue("plot", movie.Plot);
                cmd.Parameters.AddWithValue("director", movie.Director);
                cmd.Parameters.AddWithValue("actors", movie.Actors);
                cmd.Parameters.AddWithValue("language", movie.Language);
                cmd.Parameters.AddWithValue("country", movie.Country);
                cmd.Parameters.AddWithValue("poster_uri", movie.Poster);
                if (movie.Runtime.Contains("min"))
                {
                    cmd.Parameters.AddWithValue("runtime", Convert.ToInt32(movie.Runtime.Substring(0, movie.Runtime.IndexOf("m") - 1)));
                }
                else if (movie.Runtime.Contains("h"))
                {
                    cmd.Parameters.AddWithValue("runtime", Convert.ToInt32(movie.Runtime.Substring(0, movie.Runtime.IndexOf("h") - 1)) * 60);
                }
                cmd.ExecuteNonQuery();
            }
            UploadMovieGenres(id_counter, movie.Genre, connect);
        }

        public static void UploadMovieGenres(int movie_id, string genres, NpgsqlConnection connect)
        //Вставка данных о жанрах фильма в таблицу movie_genres
        {
            string insert_movie_genres_table = "INSERT INTO movie_genres VALUES (@id_movie, @id_genre)";
            string get_genre_id = "SELECT id_genre FROM genre_table WHERE genre_name = @genre";
            if ((genres != "N/A") || ((genres.Length != 0) && (genres != "N/A")))
            {
                if (genres.Contains(", "))
                {
                    String[] splitted_genre_list = genres.Split(new string[] { ", " }, StringSplitOptions.None);
                    for (int i = 0; i < splitted_genre_list.Length; i++)
                    {
                        int genre_id = new int();
                        using (NpgsqlCommand cmd = new NpgsqlCommand(get_genre_id, connect))
                        {
                            cmd.Parameters.AddWithValue("genre", splitted_genre_list[i]);
                            genre_id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        if (genre_id == 0)
                        {
                            UploadGenreData(splitted_genre_list[i], DBRowCounter("genre_table", connect) + 1, connect);
                            i--;
                            continue;
                        }
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insert_movie_genres_table, connect))
                        {
                            Console.WriteLine(String.Format("{0} {1}", movie_id, genre_id));
                            cmd.Parameters.AddWithValue("id_movie", movie_id);
                            cmd.Parameters.AddWithValue("id_genre", genre_id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    int genre_id = new int();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(get_genre_id, connect))
                    {
                        cmd.Parameters.AddWithValue("genre", genres);
                        genre_id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (genre_id == 0)
                    {
                        genre_id = DBRowCounter("genre_table", connect) + 1;
                        UploadGenreData(genres, genre_id, connect);
                    }
                    using (NpgsqlCommand cmd = new NpgsqlCommand(insert_movie_genres_table, connect))
                    {
                        cmd.Parameters.AddWithValue("id_movie", movie_id);
                        cmd.Parameters.AddWithValue("id_genre", genre_id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void UploadMovieListData(List<Movie> data_set, NpgsqlConnection connect)
        //Вставка данных о фильмах в таблицу kino
        {
            int last_film_id = DBRowCounter("kino", connect) + 1;
            for (int i = 0; i < data_set.Count; i++)
            {
                UploadMovieData(data_set[i], last_film_id, connect);
                last_film_id++;
            }
        }

        public static void UploadGenreData(string genre_name, int id_counter, NpgsqlConnection connect)
        //Вставка данных об одном жанре в таблицу genre_table
        {
            string insert_genre_table = "INSERT INTO genre_table VALUES (@id, @genre)";
            using (NpgsqlCommand cmd = new NpgsqlCommand(insert_genre_table, connect))
            {
                cmd.Parameters.AddWithValue("id", id_counter);
                cmd.Parameters.AddWithValue("genre", genre_name);
                cmd.ExecuteNonQuery();
            }
        }
        public static void UploadGenreListData(List<string> data_set, NpgsqlConnection connect)
        //Вставка данных о жанрах в таблицу genre_table
        {
            int last_genre_id = DBRowCounter("genre_table", connect) + 1;
            for (int i = 0; i < data_set.Count; i++)
            {
                UploadGenreData(data_set[i], last_genre_id, connect);
                last_genre_id++;
            }
        }

        public static Movie LoadMovieData(string movie_title, string omdb_api_key)
        //Создание экземпляра класса Movie путем десериализации json данных, поступающих от OMDB API
        {
            Movie movie = new Movie();
            WebRequest request = WebRequest.Create(String.Format("http://www.omdbapi.com/?t={0}&apikey={1}", movie_title, omdb_api_key));
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        movie = JsonConvert.DeserializeObject<Movie>(line);
                    }
                }
            }
            response.Close();
            return movie;
        }
        public static NpgsqlDataReader GetMoviesInfo(NpgsqlConnection connect)
        {
            string get_everything = "SELECT DISTINCT(title), release_date, plot, director, actors, language, " +
                "country, poster_uri, runtime, genres FROM kino as K JOIN(SELECT string_agg(genre_name, ',') " +
                "as genres, id_movie FROM movie_genres as M JOIN genre_table as G ON m.id_genre = g.id_genre GROUP " +
                "BY id_movie) as S ON K.id = S.id_movie ORDER BY title";
            using (NpgsqlCommand cmd = new NpgsqlCommand(get_everything, connect))
            {
                return cmd.ExecuteReader();
            }
        }
    }
}
