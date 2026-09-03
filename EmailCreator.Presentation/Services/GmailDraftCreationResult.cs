namespace EmailCreator.Services;

public sealed record GmailDraftCreationResult(
    string DraftId,
    string MessageId);
