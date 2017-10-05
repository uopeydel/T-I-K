﻿using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TIK.WebPortal.Helpers
{
    public static class JwtSecurityKey
    {
        public static SymmetricSecurityKey Create(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }
}
