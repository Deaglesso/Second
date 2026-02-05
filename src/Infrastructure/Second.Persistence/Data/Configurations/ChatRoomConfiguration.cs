using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Second.Domain.Entities;

namespace Second.Persistence.Data.Configurations
{
    public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.HasKey(chatRoom => chatRoom.Id);

            builder.HasIndex(chatRoom => new { chatRoom.ProductId, chatRoom.BuyerId, chatRoom.SellerId })
                .IsUnique();

            builder.HasMany(chatRoom => chatRoom.Messages)
                .WithOne(message => message.ChatRoom)
                .HasForeignKey(message => message.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
