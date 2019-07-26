using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToDoListLib
{
    public abstract class ToDoList : IEnumerable<Task>
    {
        protected abstract void AddTask(Task task);

        public void AddTask(string name)
        {
            var newTask = new Task(name);
            AddTask(newTask);
        }

        public abstract IEnumerable<Task> Find(
            string name,
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool? isCompleted = null,
            int? id = null
        );

        public bool Contains(Task task)
        {
            return Find(task.Name, task.CreationDate, task.CreationDate, task.IsCompleted, task.Id).Any();
        }

        public abstract void RemoveTask(Task task);

        public abstract void Update(Task task, string newName, bool? isCompleted = null);

        public void ToggleCompletedStatus(Task task)
        {
            Update(task, null, !task.IsCompleted);
        }

        public void RenameTask(Task task, string newName)
        {
            Update(task, newName);
        }

        public abstract void Clear();

        public abstract IEnumerator<Task> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}