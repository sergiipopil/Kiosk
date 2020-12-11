using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KioskBrains.Clients.AllegroPl.ServiceInterfaces
{
    public interface ITranslateService
    {
        Task<string> GetTranslatedText(string text);
        Task AddRecord(string textInit, string textResult, string fromCode, string toCode, Guid createdById);
        Task AddRecords(IDictionary<string, string> texts,  string fromCode, string toCode, Guid createdById);
        Task <IDictionary<string, string>> GetDictionary(IEnumerable<string> values);
        Task<IDictionary<string, string>> GetParamsDictionary();

        Task<IDictionary<string, string>> GetDescriptionDictionary();
        Task<IDictionary<string, string>> GetNamesDictionary();
    }
}
