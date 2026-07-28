using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Models.AuditModel;
using SpeakUp.API.Models.ChatModel;
using SpeakUp.API.Models.ContentModel;
using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.ResourceModel;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Models.NotificationModel;

namespace SpeakUp.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<HomePageContent> HomePageContents {  get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER → REPORTS
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Student)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.AssignedAdmin)
                .WithMany()
                .HasForeignKey(r => r.AssignedAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.LastModifiedBy)
                .WithMany()
                .HasForeignKey(r => r.LastModifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.PreviousAdmin)
                .WithMany()
                .HasForeignKey(r => r.PreviousAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // USER → CHAT CONVERSATIONS
            modelBuilder.Entity<ChatConversation>()
                .HasOne(c => c.Student)
                .WithMany(u => u.Conversations)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatConversation>()
                .HasOne(c => c.AssignedAdmin)
                .WithMany()
                .HasForeignKey(c => c.AssignedAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChatConversation>()
                .HasOne(c => c.Report)
                .WithMany()
                .HasForeignKey(c => c.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            // CHAT MESSAGE → CONVERSATION
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.ChatConversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // CHAT MESSAGE → USER (SENDER)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HomePageContent>()
                .HasOne(h => h.CreatedBy)
                .WithMany()
                .HasForeignKey(h => h.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Resource>()
                .Property(r => r.Category)
                .HasConversion<string>();
        }
    }
}