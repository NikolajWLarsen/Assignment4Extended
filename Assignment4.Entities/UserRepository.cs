using System;
using System.Collections.Generic;
using Assignment4.Core;

namespace Assignment4.Entities
{
    public class UserRepository : IUserRepository
    {
        public (Response Response, int UserId) Create(UserCreateDTO user)
        {
            throw new System.NotImplementedException();
        }

        public Response Delete(int userId, bool force = false)
        {
            throw new System.NotImplementedException();
        }

        public UserDTO Read(int userId)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<UserDTO> ReadAll()
        {
            throw new System.NotImplementedException();
        }

        public Response Update(UserUpdateDTO user)
        {
            throw new System.NotImplementedException();
        }
    }
}
