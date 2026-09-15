namespace Ifx.Web.Api.Resilience;

/// <summary>Classifies failures independently of operation replay safety.</summary>
public interface IWebApiTransientFailureClassifier
{
    /// <summary>Returns whether the failure can be transient; never authorizes replay.</summary>
    bool IsTransient(Exception exception);
}
