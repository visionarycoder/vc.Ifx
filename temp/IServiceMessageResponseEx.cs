using Eyefinity.Utilities.ErrorHandling;
using System.Collections.Generic;
using System.Linq;

namespace Eyefinity.Utilities.ServiceMessaging;

/// <summary>
/// 
/// </summary>
public static class ServiceMessageResponseEx
{

    /// <summary>
    /// Extension method to add an error to the existing response errors.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="error"></param>
    public static void AddError(this IServiceMessageResponse response, Error error)
    {

        var existing = new List<Error>(response.ResponseErrors) { error };
        response.ResponseErrors = existing.ToArray();

    }

    /// <summary>
    /// Extension method to add a collection of errors to the existing response errors.
    /// </summary>
    /// <param name="response">THe target response instance.</param>
    /// <param name="errors">A collection of errors.</param>
    public static void AddErrorRange(this IServiceMessageResponse response, ICollection<Error> errors)
    {

        var existing = response.ResponseErrors.ToList();
        existing.AddRange(errors);
        response.ResponseErrors = existing.ToArray();

    }

}