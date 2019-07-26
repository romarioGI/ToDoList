using System;

namespace ToDoListLib
{
    public class Task
    {
        public int Id { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException($"Name must be not null and not empty");

                _name = value;
            }
        }

        public bool IsCompleted { get; set; }

        public DateTime CreationDate { get; set; }

        public Task(string name)
        {
            CreationDate = DateTime.Now;
            Name = name;
            IsCompleted = false;
        }

        public Task() : this("no name") { }

        public override string ToString()
        {
            return $"Id: {Id},\tName: {Name},\tDate: {CreationDate},\tIsCompleted: {IsCompleted}";
        }
    }
}