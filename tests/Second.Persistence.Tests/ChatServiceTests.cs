using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Second.Application.Contracts.Repositories;
using Second.Application.Dtos.Requests;
using Second.Domain.Entities;
using Second.Persistence.Implementations.Services;
using Xunit;

namespace Second.Persistence.Tests
{
    public class ChatServiceTests
    {
        [Fact]
        public async Task SendMessageAsync_Throws_WhenChatRoomIsMissing()
        {
            var chatRoomRepository = new Mock<IChatRoomRepository>();
            var messageRepository = new Mock<IMessageRepository>();
            var service = new ChatService(chatRoomRepository.Object, messageRepository.Object);
            var request = new SendMessageRequest
            {
                ChatRoomId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                Content = "Hello"
            };

            chatRoomRepository
                .Setup(repository => repository.GetByIdAsync(request.ChatRoomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ChatRoom?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SendMessageAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task SendMessageAsync_PersistsMessage_WhenChatRoomExists()
        {
            var chatRoomRepository = new Mock<IChatRoomRepository>();
            var messageRepository = new Mock<IMessageRepository>();
            var service = new ChatService(chatRoomRepository.Object, messageRepository.Object);
            var chatRoomId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var request = new SendMessageRequest
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Content = "Hello"
            };

            chatRoomRepository
                .Setup(repository => repository.GetByIdAsync(chatRoomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatRoom { Id = chatRoomId });

            Message? savedMessage = null;
            messageRepository
                .Setup(repository => repository.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Callback<Message, CancellationToken>((message, _) => savedMessage = message)
                .Returns(Task.CompletedTask);

            var beforeSend = DateTime.UtcNow;

            var result = await service.SendMessageAsync(request, CancellationToken.None);

            var afterSend = DateTime.UtcNow;

            Assert.NotNull(savedMessage);
            Assert.Equal(chatRoomId, savedMessage!.ChatRoomId);
            Assert.Equal(senderId, savedMessage.SenderId);
            Assert.Equal(request.Content, savedMessage.Content);
            Assert.InRange(savedMessage.SentAt, beforeSend, afterSend);
            Assert.Equal(savedMessage.ChatRoomId, result.ChatRoomId);
            Assert.Equal(savedMessage.SenderId, result.SenderId);
            Assert.Equal(savedMessage.Content, result.Content);
        }
    }
}
