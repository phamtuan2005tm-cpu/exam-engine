using ExamEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEngine.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, string roleName);
    }
}
