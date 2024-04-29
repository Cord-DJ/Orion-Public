namespace Cord.Server.Domain;

public interface ISanitizer {
    string SanitizeHtml(string input);
}
