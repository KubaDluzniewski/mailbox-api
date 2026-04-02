using System.Reflection;

namespace Application.Email;

/// <summary>
/// Ładuje szablony HTML z zasobów wbudowanych (Embedded Resources)
/// i podmienia placeholdery {{klucz}} na rzeczywiste wartości.
/// </summary>
public static class EmailTemplates
{
    private static readonly Assembly Asm = typeof(EmailTemplates).Assembly;

    /// <summary>Szablon aktywacji konta – placeholdery: {{firstName}}, {{activationLink}}</summary>
    public static string AccountCreated(string firstName, string activationLink) =>
        Load("account-created")
            .Replace("{{firstName}}", HtmlEncode(firstName))
            .Replace("{{activationLink}}", activationLink);

    /// <summary>Powiadomienie o nowej wiadomości – placeholdery: {{recipientFirstName}}, {{senderFullName}}, {{subject}}</summary>
    public static string NewMessageNotification(string recipientFirstName, string senderFullName, string subject, string bodyHtml) =>
        Load("new-message")
            .Replace("{{recipientFirstName}}", HtmlEncode(recipientFirstName))
            .Replace("{{senderFullName}}", HtmlEncode(senderFullName))
            .Replace("{{subject}}", HtmlEncode(subject));

    /// <summary>Szablon resetu hasła – placeholdery: {{firstName}}, {{resetLink}}</summary>
    public static string PasswordReset(string firstName, string resetLink) =>
        Load("password-reset")
            .Replace("{{firstName}}", HtmlEncode(firstName))
            .Replace("{{resetLink}}", resetLink);

    /// <summary>Szablon zmiany adresu e-mail – placeholdery: {{firstName}}, {{confirmationLink}}</summary>
    public static string EmailChange(string firstName, string confirmationLink) =>
        Load("email-change")
            .Replace("{{firstName}}", HtmlEncode(firstName))
            .Replace("{{confirmationLink}}", confirmationLink);

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string Load(string templateName)
    {
        // Nazwa zasobu: Application.Email.Templates.<name>.html
        var resourceName = $"Application.Email.Templates.{templateName}.html";
        using var stream = Asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Nie znaleziono szablonu e-mail: '{resourceName}'. " +
                $"Dostępne zasoby: {string.Join(", ", Asm.GetManifestResourceNames())}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string HtmlEncode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

    private static string StripHtmlAndTruncate(string html, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var clean = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty).Trim();
        return clean.Length <= maxLength ? clean : clean[..maxLength].TrimEnd() + "…";
    }
}
