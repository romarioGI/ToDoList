using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ToDoListLib;

namespace ToDoListWithSqlLib
{
    public class ToDoListWithSql : ToDoList
    {
        private static string ConnectionName => ConfigurationManager.ConnectionStrings[1].Name;
        
        private static string TableName => "dbo.Tasks";

        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;

        protected override void AddTask(Task task)
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = GetAddCommand(task, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static SqlCommand GetAddCommand(Task task, SqlConnection sqlConnection)
        {
            var addQueryString =
                $"INSERT INTO {TableName} (Name, IsCompleted, CreationDate) VALUES(@Name, @IsCompleted, @CreationDate)";

            var addCommand = new SqlCommand(addQueryString, sqlConnection);
            addCommand.Parameters.AddWithValue("Name", task.Name);
            addCommand.Parameters.AddWithValue("IsCompleted", task.IsCompleted);
            addCommand.Parameters.AddWithValue("CreationDate", task.CreationDate);

            return addCommand;
        }

        public override IEnumerable<Task> Find(
            string name,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool? isCompleted = null,
            int? id = null)
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = GetFindCommand(sqlConnection, name, minDate, maxDate, isCompleted, id))
                {
                    return ReadCommandResult(sqlCommand);
                }
            }
        }
        
        private static SqlCommand GetFindCommand(
            SqlConnection sqlConnection,
            string name,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool? isCompleted = null,
            int? id = null)
        {
            var findQueryStringBuilder = new StringBuilder($"SELECT * FROM {TableName} WHERE ");
            var andString = " and ";
            var parameters = new Dictionary<string, object>();

            if (id.HasValue)
            {
                findQueryStringBuilder.Append("Id = @Id");
                findQueryStringBuilder.Append(andString);
                parameters.Add("Id", id.Value);
            }

            if (string.IsNullOrEmpty(name) == false)
            {
                findQueryStringBuilder.Append("Name = @Name");
                findQueryStringBuilder.Append(andString);
                parameters.Add("Name", name);
            }

            if (minDate.HasValue)
            {
                findQueryStringBuilder.Append("CreationDate >= @MinDate");
                findQueryStringBuilder.Append(andString);
                parameters.Add("MinDate", minDate.Value);
            }

            if (maxDate.HasValue)
            {
                findQueryStringBuilder.Append("CreationDate <= @MaxDate");
                findQueryStringBuilder.Append(andString);
                parameters.Add("MaxDate", maxDate.Value);
            }

            if (isCompleted.HasValue)
            {
                findQueryStringBuilder.Append("IsCompleted = @IsCompleted");
                findQueryStringBuilder.Append(andString);
                parameters.Add("IsCompleted", isCompleted.Value);
            }

            findQueryStringBuilder.Remove(findQueryStringBuilder.Length - andString.Length, andString.Length);

            var findCommand = new SqlCommand(findQueryStringBuilder.ToString(), sqlConnection);

            foreach (var pair in parameters)
                findCommand.Parameters.AddWithValue(pair.Key, pair.Value);

            return findCommand;
        }

        private static IEnumerable<Task> ReadCommandResult(SqlCommand sqlCommand)
        {
            var resultList = new List<Task>();
            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                    resultList.Add(ReadSingleRow(reader));
            }

            return resultList;
        }

        private static Task ReadSingleRow(IDataRecord record)
        {
            var res = new Task
            {
                Id = (int) record["Id"],
                Name = (string) record["Name"],
                CreationDate = (DateTime) record["CreationDate"],
                IsCompleted = (bool) record["IsCompleted"]
            };

            return res;
        }

        public override void RemoveTask(Task task)
        {
            if (task is null || Contains(task) == false)
                throw new ArgumentException();

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = GetRemoveCommand(sqlConnection, task))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static SqlCommand GetRemoveCommand(SqlConnection sqlConnection, Task task)
        {
            var removeQueryString = $"DELETE FROM {TableName} WHERE Id = {task.Id}";

            var removeCommand = new SqlCommand(removeQueryString, sqlConnection);

            return removeCommand;
        }

        public override void Update(Task task, string newName, bool? isCompleted = null)
        {
            if (task is null || Contains(task) == false)
                throw new ArgumentException();

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                if (string.IsNullOrEmpty(newName) == false)
                    task.Name = newName;

                if (isCompleted.HasValue)
                    task.IsCompleted = isCompleted.Value;

                using (var sqlCommand = GetUpdateCommand(sqlConnection, task))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static SqlCommand GetUpdateCommand(SqlConnection sqlConnection, Task task)
        {
            var updateQueryString =
                $@"UPDATE {TableName}
                SET IsCompleted = @IsCompleted, Name = @Name
                WHERE Id = @Id;";

            var updateStatusCommand = new SqlCommand(updateQueryString, sqlConnection);

            updateStatusCommand.Parameters.AddWithValue("IsCompleted", task.IsCompleted);
            updateStatusCommand.Parameters.AddWithValue("Name", task.Name);
            updateStatusCommand.Parameters.AddWithValue("Id", task.Id);

            return updateStatusCommand;
        }

        public override void Clear()
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = GetClearCommand(sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static SqlCommand GetClearCommand(SqlConnection sqlConnection)
        {
            var clearQueryString = $"TRUNCATE TABLE {TableName}";

            var clearCommand = new SqlCommand(clearQueryString, sqlConnection);

            return clearCommand;
        }

        public override IEnumerator<Task> GetEnumerator()
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = GetAllCommand(sqlConnection))
                {
                    return ReadCommandResult(sqlCommand).GetEnumerator();
                }
            }
        }

        private static SqlCommand GetAllCommand(SqlConnection sqlConnection)
        {
            var getAllQueryString = $"SELECT * FROM {TableName}";

            var getAllCommand = new SqlCommand(getAllQueryString, sqlConnection);

            return getAllCommand;
        }
    }
}