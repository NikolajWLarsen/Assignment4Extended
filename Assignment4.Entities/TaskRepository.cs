using System;
using System.Collections.Generic;
using System.Linq;
using Assignment4.Core;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Entities
{
    public class TaskRepository : ITaskRepository
    {
        private KanbanContext _kanbanContext;
        public TaskRepository(KanbanContext kanbanContext)
        {
             _kanbanContext = kanbanContext;
    
        }

        public (Response Response, int TaskId) Create(TaskCreateDTO task)
        {            
            /*if(_kanbanContext.Tasks.FirstOrDefault(t => t.Title == task.Title) != null)
            {
                return (Response.Conflict, -1);
            }*/

            if (task.AssignedToId == null) {
                return (Response.BadRequest, -1);
            }

            var newTask = new Task{
                Title = task.Title,
                AssignedTo = (int) task.AssignedToId,
                Description = task.Description,
                Created = DateTime.UtcNow,
                State = State.New,
                Tags = new List<Tag>(), //task.Tags
                StateUpdated = DateTime.UtcNow    
            };
            
            _kanbanContext.Tasks.Add(newTask);
            _kanbanContext.SaveChanges();

            return (Response.Created, newTask.Id);
        }

        public Response Delete(int taskId)
        {
            var task = _kanbanContext.Tasks.Find(taskId);
            State state = task.State;
            
            switch (state)
            {
                case State.Active: 
                    task.State = State.Removed;
                    task.StateUpdated = DateTime.UtcNow;
                    _kanbanContext.SaveChanges();
                    return Response.Updated;
                case State.Resolved:
                case State.Closed:
                case State.Removed:
                    return Response.Conflict;
                case State.New:
                    _kanbanContext.Remove(task);
                    _kanbanContext.SaveChanges();
                    return Response.Deleted;
                default:
                    return Response.NotFound;
            }
        }

        public TaskDetailsDTO Read(int taskId)
        {
            /* //string Description, DateTime Created, string AssignedToName, IReadOnlyCollection<string> Tags, 
            //State State, DateTime StateUpdated) : TaskDTO(Id, Title, AssignedToName, Tags, State);
            var task = _kanbanContext.Tasks.Find(taskId);
            return taskId != null ? new TaskDetailsDTO(task.Id, task.Title, task.Description, task.Created, "Bobby", task.Tags.AsReadOnly().ToList(), task.State, task.StateUpdated ) : null ;
         */
        return null;
         } 


        public IReadOnlyCollection<TaskDTO> ReadAll()
        {
            var TaskDTOs = new List<TaskDTO>(); 
            var taskList = _kanbanContext.Tasks.ToList();
            
            foreach (var task in taskList)
            {
                var tagList = new List<string>();
                foreach (var tag in task.Tags)
                {
                    tagList.Add(tag.Name);
                }
                //TODO: What about Bobby? We have not implemented user
                var taskDTO = new TaskDTO(task.Id, task.Title, "Bobby", tagList, task.State); 

                TaskDTOs.Add(taskDTO);
            }
            return TaskDTOs;
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
        {
            throw new System.NotImplementedException();
        }

        public Response Update(TaskUpdateDTO task)
        {
            var taskResult =_kanbanContext.Tasks.Include(t => t.Tags).FirstOrDefault(t => t.Id == task.Id);
            
            if (taskResult == null){
                return Response.NotFound;
            }

            if (task.AssignedToId == null) {
                return Response.BadRequest;
            }

            taskResult.Title = task.Title;
            taskResult.AssignedTo = (int) task.AssignedToId;
            taskResult.Description = task.Description;
            var tagResults = taskResult.Tags.Where(t => task.Tags.Contains(t.Name)).ToList();
            taskResult.Tags = tagResults; 
            taskResult.State = task.State;
            taskResult.StateUpdated = DateTime.UtcNow;
 
            _kanbanContext.Update(taskResult);
            _kanbanContext.SaveChanges();
            

            return Response.Updated;
        }
    }
}
