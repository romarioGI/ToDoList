using System;
using System.Collections.Generic;
using ToDoListLib;
using System.Data.Entity;
using System.Linq;

namespace ToDoListWithEFLib
{
    internal class TaskContext : DbContext
    {
        public TaskContext()
            : base("DBConnection")
        {
        }

        public DbSet<Task> Tasks { get; set; }
    }

    public class ToDoListWithEf : ToDoList
    {
        protected override void AddTask(Task task)
        {
            using (var taskContext = new TaskContext())
            {
                taskContext.Tasks.Add(task);
                taskContext.SaveChanges();
            }
        }

        public override IEnumerable<Task> Find(
            string name,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool? isCompleted = null,
            int? id = null
        )
        {
            using (var taskContext = new TaskContext())
            {
                if (id.HasValue)
                    return new[] { taskContext.Tasks.Find(id.Value) };

                IQueryable<Task> res = taskContext.Tasks;

                if (isCompleted.HasValue)
                    res = res.Where(x => x.IsCompleted == isCompleted.Value);

                if (minDate.HasValue)
                    res = res.Where(x => x.CreationDate >= minDate.Value);

                if (maxDate.HasValue)
                    res = res.Where(x => x.CreationDate <= maxDate.Value);

                if (string.IsNullOrEmpty(name) == false)
                    res = res.Where(x => x.Name == name);

                return res.ToArray();
            }
        }

        public override void RemoveTask(Task task)
        {
            if (task is null || Contains(task) == false)
                throw new ArgumentException();

            using (var taskContext = new TaskContext())
            {
                taskContext.Entry(task).State = EntityState.Deleted;

                taskContext.SaveChanges();
            }
        }

        public override void Update(Task task, string newName, bool? isCompleted = null)
        {
            if (task is null || Contains(task) == false)
                throw new ArgumentException();

            using (var taskContext = new TaskContext())
            {
                if (string.IsNullOrEmpty(newName) == false)
                    task.Name = newName;

                if(isCompleted.HasValue)
                    task.IsCompleted = isCompleted.Value;

                taskContext.Entry(task).State = EntityState.Modified;

                taskContext.SaveChanges();
            }
        }

        public override void Clear()
        {
            using (var taskContext = new TaskContext())
            {
                taskContext.Database.Delete();
            }
        }

        public override IEnumerator<Task> GetEnumerator()
        {
            using (var taskContext = new TaskContext())
            {
                return taskContext.Tasks.ToList().GetEnumerator();
            }
        }
    }
}