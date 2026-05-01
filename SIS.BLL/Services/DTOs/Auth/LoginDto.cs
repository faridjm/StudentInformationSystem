using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.DTOs.Auth
{
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }

    }

}
