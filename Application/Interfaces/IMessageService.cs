using Application.DTOs;
using Core.Entity;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IMessageService
{
    /// <summary>
    ///     Zapisuje wiadomość bezpośrednio do bazy. Ustawia datę wysłania jeśli nie jest podana.
    ///     Nisko-poziomowa metoda używana wewnętrznie i w testach.
    /// </summary>
    /// <param name="message">Encja wiadomości do zapisania.</param>
    Task SendMessageAsync(Message message);

    /// <summary>
    ///     Pobiera skrzynkę odbiorczą użytkownika jako listę <see cref="MessageViewDto"/>.
    ///     Zawiera status odczytu i dane nadawcy.
    /// </summary>
    /// <param name="userId">Id zalogowanego użytkownika.</param>
    Task<List<MessageViewDto>> GetInboxViewAsync(int userId);

    /// <summary>
    ///     Pobiera wiadomości wysłane przez użytkownika jako listę <see cref="MessageViewDto"/>.
    ///     Zawiera listę odbiorców z ich statusem odczytu.
    /// </summary>
    /// <param name="userId">Id zalogowanego użytkownika.</param>
    Task<List<MessageViewDto>> GetSentViewAsync(int userId);

    /// <summary>
    ///     Pobiera wersje robocze użytkownika jako listę <see cref="MessageViewDto"/>.
    ///     Rozwiązuje nazwy zarówno użytkowników jak i grup.
    /// </summary>
    /// <param name="userId">Id zalogowanego użytkownika.</param>
    Task<List<MessageViewDto>> GetDraftsViewAsync(int userId);

    /// <summary>
    ///     Pobiera pojedynczą wersję roboczą jako <see cref="MessageViewDto"/>.
    ///     Zwraca null jeśli wersja robocza nie istnieje lub nie należy do użytkownika.
    /// </summary>
    /// <param name="draftId">Id wersji roboczej.</param>
    /// <param name="userId">Id zalogowanego użytkownika.</param>
    Task<MessageViewDto?> GetDraftViewAsync(int draftId, int userId);

    /// <summary>
    ///     Wysyła wiadomość do odbiorców (użytkownicy i grupy), usuwa wersję roboczą jeśli podano Id,
    ///     i wysyła powiadomienia e-mail. Zwraca false jeśli nadawca nie istnieje lub jest nieaktywny.
    /// </summary>
    /// <param name="dto">Dane wiadomości do wysłania.</param>
    /// <param name="senderId">Id nadawcy.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    /// <param name="attachments">Opcjonalne załączniki (nadpisują załączniki wersji roboczej jeśli podano).</param>
    Task<bool> SendMessages(SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null);

    /// <summary>
    ///     Parsuje dane z formularza multipart i wysyła wiadomość.
    ///     Rzuca <see cref="ArgumentException"/> gdy brak tematu lub odbiorców.
    /// </summary>
    /// <param name="subject">Temat wiadomości.</param>
    /// <param name="body">Treść wiadomości.</param>
    /// <param name="recipients">JSON z listą odbiorców.</param>
    /// <param name="id">Opcjonalne Id wersji roboczej do usunięcia po wysłaniu.</param>
    /// <param name="attachments">Pliki załączników z formularza.</param>
    /// <param name="senderId">Id nadawcy.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    Task<bool> SendFromFormAsync(string? subject, string? body, string? recipients, int? id, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken);

    /// <summary>
    ///     Parsuje dane z formularza multipart, zapisuje wersję roboczą i zwraca <see cref="MessageViewDto"/>.
    ///     Rzuca <see cref="ArgumentException"/> przy błędnym formacie odbiorców.
    /// </summary>
    /// <param name="subject">Temat.</param>
    /// <param name="body">Treść.</param>
    /// <param name="recipients">JSON z listą odbiorców.</param>
    /// <param name="attachments">Pliki załączników z formularza.</param>
    /// <param name="senderId">Id właściciela wersji roboczej.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    Task<MessageViewDto> SaveDraftFromFormAsync(string? subject, string? body, string? recipients, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken);

    /// <summary>
    ///     Parsuje dane z formularza multipart i aktualizuje istniejącą wersję roboczą.
    ///     Zwraca null jeśli wersja robocza nie istnieje lub nie należy do użytkownika.
    ///     Rzuca <see cref="ArgumentException"/> przy błędnym formacie odbiorców.
    /// </summary>
    /// <param name="draftId">Id wersji roboczej do zaktualizowania.</param>
    /// <param name="subject">Temat.</param>
    /// <param name="body">Treść.</param>
    /// <param name="recipients">JSON z listą odbiorców.</param>
    /// <param name="attachments">Pliki załączników z formularza (null = zachowaj stare).</param>
    /// <param name="senderId">Id właściciela wersji roboczej.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    Task<MessageViewDto?> UpdateDraftFromFormAsync(int draftId, string? subject, string? body, string? recipients, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken);

    /// <summary>
    ///     Aktualizuje istniejącą wersję roboczą na podstawie <see cref="SendMessageDto"/>.
    ///     Zwraca null jeśli wersja robocza nie istnieje lub nie należy do użytkownika.
    /// </summary>
    /// <param name="draftId">Id wersji roboczej.</param>
    /// <param name="dto">Nowe dane wersji roboczej.</param>
    /// <param name="senderId">Id właściciela.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    /// <param name="attachments">Nowe załączniki (null = zachowaj stare).</param>
    Task<Message?> UpdateDraftAsync(int draftId, SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null);

    /// <summary>
    ///     Zapisuje nową wersję roboczą i zwraca encję <see cref="Message"/>.
    ///     Używana wewnętrznie i w testach.
    /// </summary>
    /// <param name="dto">Dane wersji roboczej.</param>
    /// <param name="senderId">Id właściciela.</param>
    /// <param name="cancellationToken">Token anulowania.</param>
    /// <param name="attachments">Opcjonalne załączniki.</param>
    Task<Message> SaveDraftAsync(SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null);

    /// <summary>
    ///     Usuwa wersję roboczą. Zwraca false jeśli nie istnieje lub nie należy do użytkownika.
    /// </summary>
    /// <param name="draftId">Id wersji roboczej.</param>
    /// <param name="userId">Id właściciela.</param>
    Task<bool> DeleteDraftAsync(int draftId, int userId);

    /// <summary>
    ///     Usuwa wysłaną wiadomość. Zwraca false jeśli nie istnieje, jest wersją roboczą
    ///     lub nie należy do użytkownika.
    /// </summary>
    /// <param name="messageId">Id wiadomości.</param>
    /// <param name="userId">Id nadawcy.</param>
    Task<bool> DeleteSentMessageAsync(int messageId, int userId);

    /// <summary>
    ///     Oznacza wiadomość jako przeczytaną dla danego użytkownika.
    ///     Zwraca false jeśli wiadomość lub rekord odbiorcy nie istnieje.
    /// </summary>
    /// <param name="messageId">Id wiadomości.</param>
    /// <param name="userId">Id użytkownika.</param>
    Task<bool> MarkAsReadAsync(int messageId, int userId);

    /// <summary>
    ///     Oznacza wiadomość jako nieprzeczytaną dla danego użytkownika.
    ///     Zwraca false jeśli wiadomość lub rekord odbiorcy nie istnieje.
    /// </summary>
    /// <param name="messageId">Id wiadomości.</param>
    /// <param name="userId">Id użytkownika.</param>
    Task<bool> MarkAsUnreadAsync(int messageId, int userId);

    /// <summary>
    ///     Pobiera liczbę nieprzeczytanych wiadomości użytkownika.
    /// </summary>
    /// <param name="userId">Id użytkownika.</param>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    ///     Pobiera wszystkie wiadomości w systemie (tylko dla administratorów).
    ///     Zwraca surowe encje — używana wewnętrznie i w testach.
    /// </summary>
    Task<List<Message>> GetAllMessagesAsync();

    /// <summary>
    ///     Pobiera wszystkie wiadomości w systemie zmapowane do <see cref="MessageViewDto"/>
    ///     (tylko dla administratorów). Zawiera listę odbiorców z ich statusem odczytu.
    /// </summary>
    Task<List<MessageViewDto>> GetAdminMessagesViewAsync();

    /// <summary>
    ///     Pobiera załącznik wiadomości po sprawdzeniu czy użytkownik ma dostęp do tej wiadomości
    ///     (jest nadawcą lub odbiorcą). Zwraca null przy braku dostępu lub gdy załącznik nie istnieje.
    /// </summary>
    /// <param name="messageId">Id wiadomości.</param>
    /// <param name="attachmentId">Id załącznika.</param>
    /// <param name="userId">Id użytkownika żądającego dostępu.</param>
    Task<MessageAttachment?> GetAttachmentAsync(int messageId, int attachmentId, int userId);
}
