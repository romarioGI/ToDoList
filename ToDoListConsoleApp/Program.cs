using System;
using System.Collections.Generic;
using System.Linq;
using ToDoListLib;
using ToDoListWithEFLib;
using ToDoListWithSqlLib;
using ToDoListWithDapperLib;

namespace ToDoListConsoleApp
{
    class Program
    {
        private static ToDoList _tdl;

        private static void InitToDolist()
        {
            while (true)
            {
                Console.WriteLine(@"Какой todo лист создать:
1 - на Entity framework,
2 - на Dapper,
3 - на обычненьком Sql");
                var answer = Console.ReadLine();
            
                switch (answer)
                {
                    case "1":
                        _tdl = new ToDoListWithEf();
                        return;
                    case "2":
                        _tdl = new ToDoListWithDapper();
                        return;
                    case "3":
                        _tdl = new ToDoListWithSql();
                        return;
                }

                Console.Clear();

                Console.WriteLine("Повторите ввод");
            }
        }

        private static void AddTask(string name)
        {
            _tdl.AddTask(name);
        }

        private static Task GetTask(int id)
        {
            var tasks = _tdl.Find(null, null, null, null, id);

            return tasks.ElementAt(0);
        }

        private static IEnumerable<Task> GetTasks(string name)
        {
            return _tdl.Find(name);
        }

        private static void PrintTask(params Task[] tasks)
        {
            Console.WriteLine("Count: {0}",tasks.Length);
            foreach (var task in tasks)
            {
                Console.WriteLine(task);
            }
            Console.WriteLine("------------------------");
        }

        private static void RemoveTask(params Task[] tasks)
        {
            foreach (var task in tasks)
            {
                _tdl.RemoveTask(task);
            }
        }

        private static void PrintAll()
        {
            Console.WriteLine("All");
            foreach (var task in _tdl)
            {
                Console.WriteLine(task);
            }
            Console.WriteLine("------------------------");
        }

        static void Main(string[] args)
        {
            InitToDolist();

            var s = Console.ReadLine();
            //AddTask(s);

            //PrintAll();

            //var id = int.Parse(Console.ReadLine());
            //var t = _tdl.Find(null, null, null, null, id).ElementAt(0);
            //PrintTask(t);
            //RemoveTask(t);

            //Console.Clear();

            //s = Console.ReadLine();
            //var S = Console.ReadLine();

            //foreach (var tt in _tdl.Find(s))
            //{
            //    _tdl.RenameTask(tt,S);
            //}

            foreach (var t in _tdl.Find(s))
            {
                Console.WriteLine(t);
            }
        }
    }
}
