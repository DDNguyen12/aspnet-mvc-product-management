using System;
using System.Collections.Generic;

public static class TokenStore
{
    public static Dictionary<string, DateTime> Tokens = new();

    public static string CreateToken()
    {
        var token = Guid.NewGuid().ToString();

        Tokens[token] = DateTime.Now.AddMinutes(5); // hết hạn 5 phút

        return token;
    }

    public static bool IsValid(string token)
    {
        if (!Tokens.ContainsKey(token))
            return false;

        if (Tokens[token] < DateTime.Now)
        {
            Tokens.Remove(token);
            return false;
        }

        return true;
    }
}