using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assignment4.Core;

namespace Assignment4.Entities.Tests
{
    public class TagRepositoryTests
    {
        //setup database
        private readonly KanbanContext _context;
        private readonly TagRepository _repo;

        public TagRepositoryTests()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var builder = new DbContextOptionsBuilder<KanbanContext>();
            builder.UseSqlite(connection);
            var context = new KanbanContext(builder.Options);
            context.Database.EnsureCreated();
            //add the data here
            //context.Tasks.Add(new Task {Id = 1, Title = "First task", AssignedTo = 1, Description = "The very first task", State = State.New, Tags = new List<>()});
            //context.Tags.Add(new Tag {id = 1, name = "very  good tag", Task = new List<Task>()});
            //context.Tags.Add(new Tag {id = 2, name = "almost as good as the other tag", TaskRepositoryTests = new List<Task> {}});
            context.SaveChanges();

            _context = context;
            _repo = new TagRepository(_context);
        }
        
        //delete method - checks if a tag is assigned to a task, something with force
        [Fact]
        public void Check_Delete_Assigned_Tag_With_Force() {
            //create tag and task, and assign
            var task = new Task {Id = 1, Title = "First task", AssignedTo = 1, Description = "The very first task", State = State.New, Tags = new List<Tag>()};
            var tag = new Tag {Id = 1, Name = "very  good tag", tasks = new List<Task>(){task}};
            task.Tags.Add(tag);

            _context.Tasks.Add(task);
            _context.Tags.Add(tag);

            _context.SaveChanges();

            //call delete method with force
            //Response Delete(int tagId, bool force = false)
            var force = true;
            _repo.Delete(1, force); 
            
            
            
            //assert it has been deleted
            var actual = _context.Tags.ToList().Count;
            var expected = 0;

            Assert.Equal(expected, actual);
            
        }
        
         [Fact]
        public void Check_Delete_Unassigned_Tag_Without_Force() {
            //create tag and task
            var task = new Task {Id = 1, Title = "First task", AssignedTo = 1, Description = "The very first task", State = State.New, Tags = new List<Tag>()};
            var tag = new Tag {Id = 1, Name = "very  good tag", tasks = new List<Task>(){}};

            _context.Tasks.Add(task);
            _context.Tags.Add(tag);

            _context.SaveChanges();


            //call delete method without force
            _repo.Delete(1);
            
            //assert it has been deleted

            var actual = _context.Tags.ToList().Count;
            var expected = 0;

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void trying_to_delete_tag_in_use_without_force_return_conflict(){
            //arrange setup a tag thats in use
            var task = new Task {Id = 1, Title = "First task", AssignedTo = 1, Description = "The very first task", State = State.New, Tags = new List<Tag>()};
            var tag = new Tag {Id = 1, Name = "very  good tag", tasks = new List<Task>(){task}};
            task.Tags.Add(tag);

            _context.Tasks.Add(task);
            _context.Tags.Add(tag);

            _context.SaveChanges();

            //act try to delete without using the force 
            var force = false;
            
            var actual = _repo.Delete(1, force);
            var expected = Response.Conflict;
            
            //assert respone is conflict
            Assert.Equal(expected, actual);
        }    


        //  creating a tag which already exist should return conflct
        [Fact]
        public void Create_Tag_already_existing_return_Conflict()
        {
            //arrange
            var tag = new TagCreateDTO { Name = "very  good tag"};
            _repo.Create(tag);
            
            //act
            var actual = _repo.Create(tag);
            
            //assert
            var expected = Response.Conflict;
            Assert.Equal(expected, actual.Item1);
        }


         [Fact]
         public void if_tag_not_found_return_null()
         {
            //Arrange
            var tag = new Tag {Id = 1, Name = "Tag Not Inserted", tasks = new List<Task>(){}};

            //Act
            var actual = _repo.Read(tag.Id);

            //Assert
            Assert.Null(actual);
         } 


        [Fact]
        public void ReadAll_returns_IReadOnlyCollection_containing_TagDTO_1_2()
        {
            //arrange
            var tag1 = new Tag {Id = 1, Name = "tag 1", tasks = new List<Task>(){}};
            var tag2 = new Tag {Id = 2, Name = "tag 2", tasks = new List<Task>(){}};
            
            _context.Tags.Add(tag1); 
            _context.Tags.Add(tag2);

            _context.SaveChanges();

            var tagDTO1 = new TagDTO(1,"tag 1");
            var tagDTO2 = new TagDTO(2, "tag 2");

            //act
            var actual = _repo.ReadAll();

            //assert
             Assert.Collection(actual,
                t => Assert.Equal(tagDTO1, t),
                t => Assert.Equal(tagDTO2, t)
             );
        }

        [Fact]
        public void update_updates_given_existing_tag()
        {
        //Arrange
        var tag1 = new Tag {Id = 1, Name = "tag 1", tasks = new List<Task>(){}};
        var tagDTO2 = new TagUpdateDTO{Id = 1, Name = "tag 2"};
        _context.Tags.Add(tag1);
        _context.SaveChanges();
        
        //Act
        var actual = _repo.Update(tagDTO2);
        
        //Assert
        Assert.Equal(Response.Updated, actual);
        }

        
        [Fact]
        public void update_returns_not_found_given_non_existing_tag()
        {
        //Arrange
        var tagDTO2 = new TagUpdateDTO{Id = 1, Name = "tag 2"};
        
        //Act
        var actual = _repo.Update(tagDTO2);
        
        //Assert
        Assert.Equal(Response.NotFound, actual);
        }
    }
}
