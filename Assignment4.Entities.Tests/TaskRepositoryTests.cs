using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assignment4.Core;
using Assignment4.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Assignment4.Entities.Tests
{
    public class TaskRepositoryTests
    {
        //setup database
        private readonly KanbanContext _context;
        private readonly TaskRepository _repo;

        public TaskRepositoryTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(connection);
            var context = new KanbanContext(builder.Options);
            context.Database.EnsureCreated();

            context.SaveChanges();

            _context = context;
            _repo = new TaskRepository(_context);
        }

        [Fact]
        public void createTask_Not_Already_there_returns_Response_created_and_1_And_StateTime()
        {
            //arrange
            /*var newTask = new Task{
                Title = "Task1",
                AssignedTo = (int) 1,
                Description = "This is a new task",
                Created = DateTime.UtcNow,
                State = State.New,
                Tags = new List<Tag>(),
                StateUpdated = DateTime.UtcNow   
            };*/
            
            var taskDTO = new TaskCreateDTO { 
                Title = "New task", 
                AssignedToId = 1,
                Description = "A new task",
                Tags = new List<string>()
            };
        
            //act
            var expected = (Response.Created, 1);
            var actual = _repo.Create(taskDTO);
            _context.SaveChanges();

            var task1 = _context.Tasks.Find(1);

            var expectedTime = DateTime.UtcNow;
            var actualTime = task1.Created;

            //assert
            Assert.Equal(expected, actual);
            Assert.Equal(expectedTime, actualTime, precision: TimeSpan.FromSeconds(2));
        }

        /*[Fact]
        public void Create_Task_already_existing_return_Conflict()
        {
            //arrange
            //var tag = new TagCreateDTO { Name = "very  good tag"};
            var task = new TaskCreateDTO { 
                Title = "New task", 
                AssignedToId = 1,
                Description = "A new task",
                Tags = new List<string>()
            };
        
            _repo.Create(task); 
            
            //act
            var actual = _repo.Create(task);
            
            //assert
            var expected = Response.Conflict;
            Assert.Equal(expected, actual.Item1);
        }*/

        [Fact]
        public void delete_task_active_sets_state_to_removed_and_returns_responce_updated()
        {
            //Arrange
            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.New,
                    Tags = new List<Tag>(),
                    StateUpdated = DateTime.UtcNow   
                };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();

            //Act
            var task1 = _context.Tasks.Find(1);
            task1.State = State.Active;
            //_context.SaveChanges();

        

            var expected = Response.Updated;
            var actual = _repo.Delete(1);
            
            //Assert
            Assert.Equal(expected, actual);
            Assert.Equal(State.Removed, task1.State);
        }

        [Fact]
        public void Delete_closed_task_returns_conflict()
        {
            //Arrange
            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(),
                    StateUpdated = DateTime.UtcNow   
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();

            //Act
            var expected = Response.Conflict;
            var actual = _repo.Delete(1);
            
            //Assert
            var task1 = _context.Tasks.Find(1);
            Assert.Equal(expected, actual);
            Assert.Equal(State.Closed, task1.State);
        }

        [Fact]
        public void update_should_change_StateUpdate_to_currentTime()
        {
            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(),
                    StateUpdated = new DateTime(2021,10,10)  
            };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();    

            var TaskUpdateDTO = new TaskUpdateDTO{
                Id = 1,
                Title = "New Task", 
                AssignedToId = 1,
                Description = "This is a new task",
                Tags = new List<string>(),
                State = State.Active
            };
            
            //Act
            var expected = DateTime.UtcNow;
            var actual = _repo.Update(TaskUpdateDTO);
            
            var task1 = _context.Tasks.Find(1);


            //Assert
            Assert.Equal(expected, task1.StateUpdated, precision: TimeSpan.FromSeconds(2));

        }

        [Fact]
        public void update_should_enable_editing_tags()
        {
            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(),
                    StateUpdated = new DateTime(2021,10,10)
            };
            var tags = new List<Tag>(){            
                        new Tag {Id = 1, Name = "tag1", tasks = new List<Task>(){newTask}},
                        new Tag {Id = 2, Name = "tag2", tasks = new List<Task>(){newTask}},
                        new Tag {Id = 3, Name = "tag3", tasks = new List<Task>(){newTask}},
                    };
            newTask.Tags = tags;
            _context.Tasks.Add(newTask);
            _context.SaveChanges();    

            var taskUpdateDTO = new TaskUpdateDTO{
                Id = 1,
                Title = "New Task", 
                AssignedToId = 1,
                Description = "This is a new task",
                Tags = new List<string>(){"tag1"},
                State = State.Active
            };
            
            //Act
            _repo.Update(taskUpdateDTO); 
            _context.SaveChanges(); 
      
            var actualTask = _context.Tasks.Find(1);
            var actualTags = actualTask.Tags;
                        
            //Assert            
          
            var actualTag1 = actualTags.ToList().ElementAt(0);
            var actualLength = actualTags.ToList().Count;
            
            Assert.Equal("New Task", actualTask.Title);
            Assert.Equal(taskUpdateDTO.Title, actualTask.Title);
            Assert.Equal(taskUpdateDTO.Description, actualTask.Description);
            Assert.Equal(1, actualLength);
            Assert.Equal("tag1", actualTag1.Name);
        }

        [Fact]
        public void read_given_taskId_return_TaskDetailsDTO()
        {  
            //Arrange
             var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(),
                    StateUpdated = DateTime.UtcNow   
            };

            _context.Tasks.Add(newTask);
            _context.SaveChanges(); 

            TaskDetailsDTO exp = new TaskDetailsDTO(1,"Task1", "This is a new task", DateTime.UtcNow, "1",  new List<string>(), State.Closed, DateTime.UtcNow);

            //Act
            var actual = _repo.Read(newTask.Id);
            var expected = exp;

            //Assert
            Assert.Equal(expected.Title, actual.Title);
            Assert.False(actual.Tags.Any());
            Assert.Equal(expected.Created, actual.Created, precision: TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ReadAll_returns_IReadOnlyCollection_containing_TaskDTO_1_2()
        {
            //arrange
            var task1 = new Task {
                Id = 1, 
                Title = "task 1", 
                AssignedTo = 1, 
                Description = "The first task", 
                Created = DateTime.UtcNow,
                State = State.New,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var task2 = new Task {
                Id = 2, 
                Title = "task 2", 
                AssignedTo = 2, 
                Description = "The second task", 
                Created = DateTime.UtcNow,
                State = State.Closed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            
            _context.Tasks.Add(task1); 
            _context.Tasks.Add(task2); 

            _context.SaveChanges();


            //public record TaskDTO(int Id, string Title, string AssignedToName, IReadOnlyCollection<string> Tags, State State);
            var taskDTO1 = new TaskDTO(1, "task 1", "1", new List<string>(), State.New);
            var taskDTO2 = new TaskDTO(2, "task 2", "2", new List<string>(), State.Closed);

    
            //act
            var actual = _repo.ReadAll();

            //assert
            Assert.Collection(actual,
                t => Assert.False(t.Tags.ToList().Any()),
                t => Assert.False(t.Tags.ToList().Any())
            );
            Assert.Collection(actual,
                t => Assert.Equal(taskDTO1.Title, t.Title),
                t => Assert.Equal(taskDTO2.Title, t.Title)
            );
            Assert.Collection(actual,
                t => Assert.Equal(taskDTO1.State, t.State),
                t => Assert.Equal(taskDTO2.State, t.State)
            );
        }

        [Fact]
        public void ReadAllByState_returns_IReadOnlyCollection_containing_TaskDTO_1()
        {
            //arrange
            var task1 = new Task {
                Id = 1, 
                Title = "task 1", 
                AssignedTo = 1, 
                Description = "The first task", 
                Created = DateTime.UtcNow,
                State = State.New,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var task2 = new Task {
                Id = 2, 
                Title = "task 2", 
                AssignedTo = 2, 
                Description = "The second task", 
                Created = DateTime.UtcNow,
                State = State.Closed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            
            _context.Tasks.Add(task1); 
            _context.Tasks.Add(task2); 

            _context.SaveChanges();

            var expected = new TaskDTO(1, "task 1", "1", new List<string>(), State.New);
    
            //act
            var actual = _repo.ReadAllByState(State.New);

            //assert
            Assert.Collection(actual,
                t => Assert.False(t.Tags.ToList().Any())
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.Title, t.Title)
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.State, t.State)
            );
        }

        [Fact]
        public void ReadAllByTag_returns_IReadOnlyCollection_containing_TaskDTO_2()
        {
            //arrange
            var task1 = new Task {
                Id = 1, 
                Title = "task 1", 
                AssignedTo = 1, 
                Description = "The first task", 
                Created = DateTime.UtcNow,
                State = State.New,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var task2 = new Task {
                Id = 2, 
                Title = "task 2", 
                AssignedTo = 2, 
                Description = "The second task", 
                Created = DateTime.UtcNow,
                State = State.Closed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var tag1 = new Tag {Id = 1, Name = "tag1", tasks = new List<Task>(){task1}};
            var tag2 = new Tag {Id = 2, Name = "tag2", tasks = new List<Task>(){task2}};

            task1.Tags.Add(tag1);
            task2.Tags.Add(tag2);
            
            _context.Tasks.Add(task1); 
            _context.Tasks.Add(task2); 

            _context.SaveChanges();

            var expected = new TaskDTO(2, "task 2", "2", new List<string>(){"tag2"}, State.Closed);
            //Console.WriteLine(expected.Title);

    
            //act
            var actual = _repo.ReadAllByTag("tag2");

            //assert
            Assert.Collection(actual,
                t => Assert.True(t.Tags.ToList().Any())
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.Title, t.Title)
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.State, t.State)
            );

        }

        [Fact]
        public void ReadAllByUser_returns_IReadOnlyCollection_containing_TaskDTO_1()
        {
            //arrange
            var task1 = new Task {
                Id = 1, 
                Title = "task 1", 
                AssignedTo = 1, 
                Description = "The first task", 
                Created = DateTime.UtcNow,
                State = State.New,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var task2 = new Task {
                Id = 2, 
                Title = "task 2", 
                AssignedTo = 2, 
                Description = "The second task", 
                Created = DateTime.UtcNow,
                State = State.Closed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            
            _context.Tasks.Add(task1); 
            _context.Tasks.Add(task2); 

            _context.SaveChanges();

            var expected = new TaskDTO(1, "task 1", "1", new List<string>(), State.New);
    
            //act
            var actual = _repo.ReadAllByUser(1);

            //assert
            Assert.Collection(actual,
                t => Assert.False(t.Tags.ToList().Any())
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.Title, t.Title)
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.AssignedToName, t.AssignedToName)
            );
        }


        [Fact]
        public void ReadAllRemoved_returns_IReadOnlyCollection_containing_TaskDTO_1()
        {
            //arrange
            var task1 = new Task {
                Id = 1, 
                Title = "task 1", 
                AssignedTo = 1, 
                Description = "The first task", 
                Created = DateTime.UtcNow,
                State = State.Removed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            var task2 = new Task {
                Id = 2, 
                Title = "task 2", 
                AssignedTo = 2, 
                Description = "The second task", 
                Created = DateTime.UtcNow,
                State = State.Closed,
                Tags =  new List<Tag>(),
                StateUpdated = DateTime.UtcNow
            };
            
            _context.Tasks.Add(task1); 
            _context.Tasks.Add(task2); 

            _context.SaveChanges();

            var expected = new TaskDTO(1, "task 1", "1", new List<string>(), State.Removed);
    
            //act
            var actual = _repo.ReadAllRemoved();

            //assert
            Assert.Collection(actual,
                t => Assert.False(t.Tags.ToList().Any())
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.Title, t.Title)
            );
            Assert.Collection(actual,
                t => Assert.Equal(expected.State, t.State)
            );
        }
    }
}
