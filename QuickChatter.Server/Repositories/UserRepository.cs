using Microsoft.Data.Sqlite;
using QuickChatter.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickChatter.Server.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    username TEXT NOT NULL UNIQUE,
                    password TEXT NOT NULL,
                    last_login TEXT
                );
            ";
            command.ExecuteNonQuery();

            // Check if the test user already exists
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM users WHERE username = 'testuser'";
            long count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                // Insert test user
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                    INSERT INTO users (id, username, password, last_login)
                    VALUES ($id, $username, $password, $last_login);
                ";
                insertCommand.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
                insertCommand.Parameters.AddWithValue("$username", "testuser");
                insertCommand.Parameters.AddWithValue("$password", "password123"); // ⚠️ plain text for testing only
                insertCommand.Parameters.AddWithValue("$last_login", DateTime.UtcNow.ToString("o"));
                insertCommand.ExecuteNonQuery();
            }
        }

        private void AddUser(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO users (id, username, password, last_login)
                VALUES ($id, $username, $password, $last_login);
            ";
            command.Parameters.AddWithValue("$id", user.Id);
            command.Parameters.AddWithValue("$username", user.Username);
            command.Parameters.AddWithValue("$password", user.Password);
            command.Parameters.AddWithValue("$last_login", user.LastLogin.ToString("o")); // ISO 8601 format

            command.ExecuteNonQuery();
        }

        private List<User> GetAllUsers()
        {
            var users = new List<User>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, username, password FROM users";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                });
            }

            return users;
        }

        private User? GetByUsername(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, username, password
                FROM users 
                WHERE username = $username
            ";
            command.Parameters.AddWithValue("$username", username);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                };
            }

            return null; // user not found
        }

        private void UpdateLastLogin(string userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE users
                SET last_login = $last_login
                WHERE id = $id;
            ";
            command.Parameters.AddWithValue("$last_login", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("$id", userId);

            command.ExecuteNonQuery();
        }

        public User? LoginUser(string username, string password)
        {
            var user = GetByUsername(username);

            if (user is not null && user.Password == password)
            {
                return user;
            }

            return null; // login failed
        }

        public bool RegisterUser(string username, string password)
        {
            //Check if the username already exsits
            var user = GetByUsername(username);

            if (user is not null)
            {
                return false;
            }

            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                LastLogin = DateTime.UtcNow,
                Password = password,
                Username = username,
            };

            AddUser(user);

            return true;
        }

    }
}
