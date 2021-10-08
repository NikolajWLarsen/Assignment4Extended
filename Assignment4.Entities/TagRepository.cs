using System;
using System.Collections.Generic;
using Assignment4.Core;
using System.Linq;

namespace Assignment4.Entities
{
    public class TagRepository : ITagRepository
    {
        private KanbanContext _context;

        

        public TagRepository(KanbanContext context)
        {
            _context = context;
        }
        
        public (Response Response, int TagId) Create(TagCreateDTO tag)
        {

            if(_context.Tags.FirstOrDefault(x => x.Name == tag.Name) == null)
            {
                return (Response.Conflict, -1);
            }
            
            var newTag = new Tag{
                Name = tag.Name    
            };

            _context.Tags.Add(newTag);
            _context.SaveChanges();
            
            return (Response.Created, newTag.Id);
               
        }

        public Response Delete(int tagId, bool force = false)
        {
            var tag = _context.Tags.Find(tagId);
            if (tag.tasks.Count > 0 ) 
            {
                if (force)
                {
                    _context.Remove(tag);
                } else 
                {
                    return Response.Conflict;
                }
                
            } else 
            {
                _context.Remove(tag); 
            }
            _context.SaveChanges();
            return Response.Deleted;
        }

        public TagDTO Read(int tagId)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            throw new System.NotImplementedException();
        }

        public Response Update(TagUpdateDTO tag)
        {
            throw new System.NotImplementedException();
        }
    }
}
