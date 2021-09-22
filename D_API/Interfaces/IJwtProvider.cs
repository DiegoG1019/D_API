using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Interfaces;

public interface IJwtProvider
{
    /// <summary>
    /// Generates a new token using the provided name and roles
    /// </summary>
    /// <param name="names"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    string? GenerateToken(string name, Guid key, TimeSpan duration, params string[] role);

    /// <summary>
    /// Generates a new token using the provided name and comma separated roles
    /// </summary>
    /// <param name="names"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    string? GenerateToken(string name, Guid key, TimeSpan duration, string role);
}
