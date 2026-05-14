using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpireMinutes { get; set; }
        public int RefreshTokenExpireDays { get; set; }
    }
}
