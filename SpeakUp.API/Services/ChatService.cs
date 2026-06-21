using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.Models.ChatModel;

namespace SpeakUp.API.Services;

public class ChatService
{
    private readonly ApplicationDbContext _context;

    public ChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatConversation?> CreateFromReportAsync(int reportId, int studentId)
    {
        var existing = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.ReportId == reportId);

        if (existing != null)
            return null;

        var conversation = new ChatConversation
        {
            ChatType = ChatType.Report,
            IsAnonymous = false,
            StudentId = studentId,
            ReportId = reportId,
            Status = ConversationStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatConversations.Add(conversation);
        await _context.SaveChangesAsync();

        return conversation;
    }
}