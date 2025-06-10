using Microsoft.AspNetCore.Mvc;

namespace RagWebScraper.Factories;

/// <summary>
/// Common interface for analysis controllers.
/// </summary>
public interface IAnalyzerController
{
    /// <summary>
    /// Invokes the analyze action on the controller.
    /// </summary>
    /// <param name="request">Medium specific request object.</param>
    /// <returns>A task containing the result.</returns>
    Task<IActionResult> AnalyzeAsync(object request);
}
