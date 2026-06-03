using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FutaMedical.Application.Common.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderAsync(string templateCode, Dictionary<string, string> templateData, CancellationToken cancellationToken = default);
}
