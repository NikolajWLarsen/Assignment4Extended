using Assignment4.Entities;
namespace Assignment4
{
    class Program
    {
        private readonly KanbanContext _context;
        private readonly TaskRepository _repo;
        static void Main(string[] args)
        {
            /* Console.WriteLine("Hello World!");
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(connection);
            var context = new KanbanContext(builder.Options);
            context.Database.EnsureCreated();

            context.SaveChanges();

            _context = context;
            _repo = new TaskRepository(_context);

            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(){            
                        new Tag {Id = 1, Name = "tag1", tasks = new List<Task>(){}},
                        new Tag {Id = 2, Name = "tag2", tasks = new List<Task>(){}},
                        new Tag {Id = 3, Name = "tag3", tasks = new List<Task>(){}},
                    },
                    StateUpdated = new DateTime(2021,10,10)
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();    

            var TaskUpdateDTO = new TaskUpdateDTO{
                Id = 1,
                Title = "New Task", 
                AssignedToId = null,
                Description = "This is a new task",
                Tags = new List<string>(){"tag1", "tag2"},
                State = State.Active
            };
            
            //Act
            var temp = _repo.Update(TaskUpdateDTO);
            
            var task1 = _context.Tasks.Find(1);
            var expected = new List<Tag>(){            
                        new Tag {Id = 1, Name = "tag1", tasks = new List<Task>(){}},
                        new Tag {Id = 2, Name = "tag2", tasks = new List<Task>(){}}
                    };
            var actual = task1.Tags; */
        }
    }
}
