using RagWebScraper.Models;

namespace RagWebScraper.ViewModels
{
    public interface IRagAnalysisResultPresenter
    {
        Task DisplayAsync(AnalysisResult result);
    }

}
