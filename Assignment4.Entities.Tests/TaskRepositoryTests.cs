using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assignment4.Core;
using Assignment4.Entities;
using System.Collections.Generic;

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
            var emptyTasks = new List<Task>(){};

            var newTask = new Task{
                    Title = "Task1",
                    AssignedTo = (int) 1,
                    Description = "This is a new task",
                    Created = DateTime.UtcNow,
                    State = State.Closed,
                    Tags = new List<Tag>(){            
                        new Tag {Id = 1, Name = "tag1", tasks = emptyTasks},
                        new Tag {Id = 2, Name = "tag2", tasks = emptyTasks},
                        new Tag {Id = 3, Name = "tag3", tasks = emptyTasks},
                    },
                    StateUpdated = new DateTime(2021,10,10)
            };
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
      
            var task1 = _context.Tasks.Find(1);
            Console.WriteLine(task1.Tags.Count);
            var actual = task1.Tags;
            
            var tag1 = new Tag {Id = 1, Name = "tag1", tasks = new List<Task>(){task1}}; //task1
            //Assert
            Assert.Equal("New Task", task1.Title);
            foreach (var t in actual)
            {
                Console.WriteLine((tag1).Name == t.Name);
            }
            //TODO: count instead
            Assert.Collection(actual,
                t => Assert.Equal((tag1), t)
                // t => Assert.Equal((tag1).Name, t.Name),
                // t => Assert.Equal((tag1).tasks, t.tasks)
                //t => Assert.Equal((tag1).Name, t.Name),
                //t => Assert.Equal(tag2, t)
            );
        }

        /* [Fact]
        public void given_taskId_return_TaskDetailsDTO()
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

           // TaskDetailsDTO exp = new TaskDetailsDTO{Title = "Task1",Description = "This is a new task", Created = DateTime.UtcNow, 
            //AssignedToName = "Bobby", List<string> list= new List<string>(), };



            //Act
            var actual = _repo.Read(newTask.Id);
            var expected = exp;

            //Assert

        }


        [Fact]
        public void ReadAll_returns_IReadOnlyCollection_containing_TaskDTO_1_2()
        {
            //arrange
            var task1 = new Tag {Id = 1, Name = "tag 1", tasks = new List<Task>(){}};
            var task2 = new Tag {Id = 2, Name = "tag 2", tasks = new List<Task>(){}};
            
            _context.Tags.Add(task1); 
            _context.Tags.Add(task2);

            _context.SaveChanges();

            var taskDTO1 = new TaskDTO(1,"tag 1");
            var taskDTO2 = new TaskDTO(2, "tag 2");

            //act
            var actual = _repo.ReadAll();

            //assert
             /* Assert.Collection(actual,
                t => Assert.Equal(tagDTO1, t),
                t => Assert.Equal(tagDTO2, t)
             ); */
        //}


    }
}
