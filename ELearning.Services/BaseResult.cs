using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELearning.Services
{
    public class BaseResult<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; } = 200;
        public static BaseResult<T> Success(T? data = default, string message = "")
        {
            return new BaseResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static BaseResult<T> Fail(List<string>? errors = null)
        {
            return new BaseResult<T>
            {
                IsSuccess = false,
                Message = "",
                Errors = errors,
                StatusCode = 400
            };
        }
    }
}
