using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EComApi.Services.Services
{
    public interface ISecurityService
    {
        Task<Result<ResponseDto>> Login(LoginDto login);
        Task<Result<ResponseDto>> Register(RequestDto request);
    }
}
