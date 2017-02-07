using System.Threading.Tasks;

namespace MileEyes.Services
{
    public interface IHttpHelper
    {
        Task<string> FileGetContents(string url);
    }
}