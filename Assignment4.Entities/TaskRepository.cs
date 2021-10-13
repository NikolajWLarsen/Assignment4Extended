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
                Tags = new List<Tag>(), //task.Tags //TODO: haha we did not
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

            var task = _kanbanContext.Tasks.Find(taskId);
            if(task == null){
                return null; //Response.NotFound
            }

            var TaskDetailsDTO = new TaskDetailsDTO (
                task.Id,
                task.Title,
                task.Description,
                task.Created,
                task.AssignedTo.ToString(),
                task.Tags.Select(t => t.Name).ToList(),
                task.State,
                task.StateUpdated                 
            );
            return TaskDetailsDTO;
         } 


        public IReadOnlyCollection<TaskDTO> ReadAll()
        {
            var taskList = _kanbanContext.Tasks.ToList();
            return TaskToTaskDTO(taskList);
        }

        //selfmade method, to avoid code duplication
        public IReadOnlyCollection<TaskDTO> TaskToTaskDTO(List<Task> taskList)
        {
            var TaskDTOs = new List<TaskDTO>(); 
            foreach (var task in taskList)
            {
                var tagList = new List<string>();
                foreach (var tag in task.Tags)
                {
                    tagList.Add(tag.Name);
                }
                var taskDTO = new TaskDTO(task.Id, task.Title, task.Id.ToString(), tagList, task.State); 

                TaskDTOs.Add(taskDTO);
            }
            return TaskDTOs;
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
        {
            var taskList = _kanbanContext.Tasks.Where(t => t.State == state).ToList();
            
            return TaskToTaskDTO(taskList);
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tagString)
        {
            //convert string tag to actual tag???
            //var taskList = _kanbanContext.Tasks.Where(t => t.Tags.Select(t => t.Name == tag)).ToList();
            var taskList = _kanbanContext.Tasks.ToList();
            var taskWithTagList = new List<Task>();

            foreach (var task in taskList)
            {
                var tagList = new List<string>();
                foreach (var tag in task.Tags)
                {
                    if(tag.Name == tagString){
                        taskWithTagList.Add(task);
                    }
                }
            }
            return TaskToTaskDTO(taskWithTagList);
        }

        public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
        {
            var taskList = _kanbanContext.Tasks.Where(t => t.AssignedTo == userId).ToList();
            
            return TaskToTaskDTO(taskList);
        }

        public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
        {
            var taskList = _kanbanContext.Tasks.Where(t => t.State == State.Removed).ToList();
            
            return TaskToTaskDTO(taskList);
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
