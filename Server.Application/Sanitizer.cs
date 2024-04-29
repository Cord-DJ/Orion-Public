using Cord.Server.Domain;
using Ganss.Xss;

namespace Cord.Server.Application;

public class Sanitizer : ISanitizer {
    readonly HtmlSanitizer sanitizer = new();

    public string SanitizeHtml(string input) => sanitizer.Sanitize(input);
}
