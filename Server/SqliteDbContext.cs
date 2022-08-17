using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Models;

namespace Server
{
    public class SqliteDbContext
    {
        private string ConnectionString = "MyUserDb.SqliteDB";


        public async Task EditUser(User user)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + ConnectionString + ";Mode=ReadWriteCreate"))
            {
                await connection.OpenAsync();

                string row = "UPDATE Users SET Name='" + user.Name + "', Email ='" + user.Email + "', Password='" + user.Password + "', Role='" + Convert.ToInt32(user.Role).ToString() + "' WHERE Id =" + user.Id;

                SqliteCommand command = new SqliteCommand(row, connection);

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Sqlite row inserted at: " + DateTime.Now);
                await connection.CloseAsync();
            }
        }

        public async Task AddUser(User user)
        {
            // Create database if not exists.
            if (!File.Exists("MyUserDb.SqliteDB"))
                await CreateTable();

            using (SqliteConnection connection = new SqliteConnection("Data Source=" + ConnectionString + ";Mode=ReadWriteCreate"))
            {
                await connection.OpenAsync();

                string row = "INSERT INTO Users(Name, Email, Password, Role) VALUES('" + user.Name + "', '" + user.Email + "', '" + user.Password + "'," + Convert.ToInt32(user.Role).ToString() + ")";
                SqliteCommand command = new SqliteCommand(row, connection);

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Sqlite row inserted at: " + DateTime.Now);
                await connection.CloseAsync();
            }
        }

        public async Task<User> GetUser(User user)
        {

            using (SqliteConnection connection = new SqliteConnection("Data Source=" + ConnectionString + ";Mode=ReadWriteCreate"))
            {
                await connection.OpenAsync();

                string selectString = "SELECT * FROM Users WHERE Users.Email = '" + user.Email + "' AND Users.Password = '" + user.Password + "'";
                SqliteCommand command = new SqliteCommand(selectString, connection);

                await command.ExecuteNonQueryAsync();

                Console.WriteLine("Sqlite data selected at: " + DateTime.Now);

                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Email = reader.GetString(2);
                    user.Password = reader.GetString(3);
                    user.Role = (Roles)reader.GetInt32(4);
                }
                await connection.CloseAsync();

                if (user.Id <= 0)
                {
                    user = null;
                }

                return user;
            }

        }

        public async Task<User> GetUserById(int id)
        {
            User user = new User();

            using (SqliteConnection connection = new SqliteConnection("Data Source=" + ConnectionString + ";Mode=ReadWriteCreate"))
            {
                await connection.OpenAsync();
                string selectString = "SELECT * FROM Users WHERE Users.Id = '" + id.ToString() + "' LIMIT 1";
                SqliteCommand command = new SqliteCommand(selectString, connection);
                await command.ExecuteNonQueryAsync();
                user = await ReadUser(command);
                Console.WriteLine("Sqlite data selected at: " + DateTime.Now);
                await connection.CloseAsync();
                return user;
            }
        }

        private async Task CreateTable()
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + ConnectionString + ";Mode=ReadWriteCreate"))
            {
                await connection.OpenAsync();
                string sqlText = "CREATE TABLE IF NOT EXISTS Users(Id INTEGER PRIMARY KEY, Name varchar(255), Email varchar(255), Password varchar(255), Role int);";
                SqliteCommand command = new SqliteCommand(sqlText, connection);
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Sqlite table created at: " + DateTime.Now);
                await connection.CloseAsync();
            }

        }

        private async Task<User> ReadUser(SqliteCommand command)
        {
            SqliteDataReader reader = await command.ExecuteReaderAsync();
            User user = new User();
            while (reader.Read())
            {
                user.Id = reader.GetInt32(0);
                user.Name = reader.GetString(1);
                user.Email = reader.GetString(2);
                user.Password = reader.GetString(3);
                user.Role = (Roles)reader.GetInt32(4);
            }

            return user;
        }
    }
}
