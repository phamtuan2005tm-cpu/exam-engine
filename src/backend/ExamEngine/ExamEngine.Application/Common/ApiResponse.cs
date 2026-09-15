using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEngine.Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; } 
        public List<string> Errors { get; set; }
        
        public static ApiResponse<T> SuccessResult (T data, string Message = "Thành Công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = Message,
                Data = data,
                Errors = null
            };
        }

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors
            };
        }
    }
}
