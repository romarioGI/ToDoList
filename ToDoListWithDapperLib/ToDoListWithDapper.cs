using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using ToDoListLib;

namespace ToDoListWithDapperLib
{
    public class ToDoListWithDapper : ToDoList
    {
        private static string ConnectionName => ConfigurationManager.ConnectionStrings[1].Name;

        private static string TableName => "dbo.Tasks";

        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;

        protected override void AddTask(Task task)
        {
            var addQueryString =
                $"INSERT INTO {TableName} (Name, IsCompleted, CreationDate) VALUES(@Name, @IsCompleted, @CreationDate)";

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                sqlConnection.Execute(addQueryString, new {task.Name, task.IsCompleted, task.CreationDate});
            }
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
                var findQueryString = GetFindQueryString(out var parameters, name, minDate, maxDate, isCompleted, id);
                var findResult = sqlConnection.Query<Task>(findQueryString, parameters).ToList();

                return findResult;
            }
        }

        private static string GetFindQueryString(
            out object parameters,
            string name,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool? isCompleted = null,
            int? id = null)
        {
            var findQueryStringBuilder = new StringBuilder($"SELECT * FROM {TableName} WHERE ");
            var andString = " and ";

            parameters = new {Id = id, Name = name, MinDate = minDate, MaxDate = maxDate, IsCompleted = isCompleted};

            if (id.HasValue)
            {
                findQueryStringBuilder.Append("Id = @Id");
                findQueryStringBuilder.Append(andString);
            }

            if (string.IsNullOrEmpty(name) == false)
            {
                findQueryStringBuilder.Append("Name = @Name");
                findQueryStringBuilder.Append(andString);
            }

            if (minDate.HasValue)
            {
                findQueryStringBuilder.Append("CreationDate >= @MinDate");
                findQueryStringBuilder.Append(andString);
            }

            if (maxDate.HasValue)
            {
                findQueryStringBuilder.Append("CreationDate <= @MaxDate");
                findQueryStringBuilder.Append(andString);
            }

            if (isCompleted.HasValue)
            {
                findQueryStringBuilder.Append("IsCompleted = @IsCompleted");
                findQueryStringBuilder.Append(andString);
            }

            findQueryStringBuilder.Remove(findQueryStringBuilder.Length - andString.Length, andString.Length);

            return findQueryStringBuilder.ToString();
        }

        public override void RemoveTask(Task task)
        {
            if (task is null || Contains(task) == false)
                throw new ArgumentException();

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                var removeQueryString = $"DELETE FROM {TableName} WHERE Id = @Id";

                sqlConnection.Execute(removeQueryString, new {task.Id});
            }
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

                var updateQueryString =
                    $@"UPDATE {TableName}
                    SET IsCompleted = @IsCompleted, Name = @Name
                    WHERE Id = @Id;";

                sqlConnection.Execute(updateQueryString, new {task.IsCompleted, task.Name, task.Id});
            }
        }

        public override void Clear()
        {
            var clearQueryString = $"TRUNCATE TABLE {TableName}";

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                sqlConnection.Execute(clearQueryString);
            }
        }

        public override IEnumerator<Task> GetEnumerator()
        {
            var getAllQueryString = $"SELECT * FROM {TableName}";

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                var orderDetails = sqlConnection.Query<Task>(getAllQueryString).ToList();

                return orderDetails.GetEnumerator();
            }
        }
    }
}