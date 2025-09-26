using CondoManager.Api.DTOs.Chat;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Interfaces;
using CondoManager.Entity.Events;
using CondoManager.Entity.Models;
using CondoManager.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RabbitMqPublisher _eventPublisher;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IUnitOfWork unitOfWork, RabbitMqPublisher eventPublisher, ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<MessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            // Check if user exists, if not create a test user
            var userExists = await _unitOfWork.Users.GetByIdAsync(request.SenderId);
            if (userExists == null)
            {
                var testUser = new User
                {
                    Id = request.SenderId,
                    Name = "Test User",
                    Email = "test@example.com",
                    Phone = "1234567890",
                    PasswordHash = "hashedpassword",
                    Roles = Entity.Enums.UserRole.Resident,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(testUser);
                await _unitOfWork.SaveChangesAsync();
            }

            // Create a conversation first
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = Entity.Enums.ConversationType.Direct,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Conversations.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            // Create message
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = request.SenderId,
                ConversationId = conversation.Id, // Use the created conversation ID
                ApartmentId = request.ApartmentId,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                Type = request.IsAnnouncement ? Entity.Enums.MessageType.Announcement : Entity.Enums.MessageType.Text,
                ChatType = request.ApartmentId.HasValue ? Entity.Enums.ChatType.ApartmentGroup : Entity.Enums.ChatType.Direct
            };

            // Save message to database
            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            // Publish message sent event
            var messageSentEvent = new MessageSentEvent
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ApartmentId = message.ApartmentId,
                Content = message.Content,
                SentAt = message.SentAt
            };
            await _eventPublisher.PublishAsync(messageSentEvent, "message.sent");

            _logger.LogInformation("Message {MessageId} sent by user {SenderId}", message.Id, request.SenderId);

            // Return response
            return new MessageResponse
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ApartmentId = message.ApartmentId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsAnnouncement = request.IsAnnouncement
            };
        }

        public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid? apartmentId)
        {
            var messages = await _unitOfWork.Messages.GetByApartmentIdAsync(apartmentId);
            
            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ApartmentId = m.ApartmentId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsAnnouncement = m.Type == Entity.Enums.MessageType.Announcement
            });
        }

        public async Task<IEnumerable<MessageResponse>> GetUserMessagesAsync(Guid userId)
        {
            var messages = await _unitOfWork.Messages.GetBySenderIdAsync(userId);
            
            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ApartmentId = m.ApartmentId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsAnnouncement = m.Type == Entity.Enums.MessageType.Announcement
            });
        }

        public async Task<IEnumerable<ConversationResponse>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _unitOfWork.Conversations.GetUserConversationsAsync(userId);
            var conversationResponses = new List<ConversationResponse>();

            foreach (var conversation in conversations)
            {
                var lastMessage = await _unitOfWork.Messages.GetLastMessageByConversationIdAsync(conversation.Id);
                var unreadCount = await _unitOfWork.Messages.GetUnreadCountAsync(conversation.Id, userId);
                var participants = await _unitOfWork.ConversationParticipants.GetByConversationIdAsync(conversation.Id);

                var conversationResponse = new ConversationResponse
                {
                    Id = conversation.Id,
                    Type = conversation.Type,
                    Name = conversation.Name,
                    ApartmentId = conversation.ApartmentId,
                    CreatedAt = conversation.CreatedAt,
                    LastMessageAt = lastMessage?.SentAt,
                    IsActive = conversation.IsActive,
                    UnreadCount = unreadCount,
                    LastMessage = lastMessage != null ? new MessageResponse
                    {
                        Id = lastMessage.Id,
                        SenderId = lastMessage.SenderId,
                        ApartmentId = lastMessage.ApartmentId,
                        Content = lastMessage.Content,
                        SentAt = lastMessage.SentAt,
                        IsAnnouncement = lastMessage.Type == Entity.Enums.MessageType.Announcement
                    } : null,
                    Participants = participants.Select(p => new ConversationParticipantResponse
                    {
                        UserId = p.UserId,
                        UserName = p.User?.Name ?? "Unknown",
                        UserEmail = p.User?.Email ?? "Unknown",
                        IsAdmin = p.IsAdmin,
                        JoinedAt = p.JoinedAt
                    }).ToList()
                };

                conversationResponses.Add(conversationResponse);
            }

            return conversationResponses.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);
        }

        public async Task<IEnumerable<MessageResponse>> GetConversationMessagesAsync(Guid conversationId, int skip = 0, int take = 50)
        {
            var messages = await _unitOfWork.Messages.GetByConversationIdAsync(conversationId, skip, take);
            
            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ApartmentId = m.ApartmentId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsAnnouncement = m.Type == Entity.Enums.MessageType.Announcement
            });
        }

        public async Task<bool> IsUserInConversationAsync(Guid userId, Guid conversationId)
        {
            var participant = await _unitOfWork.ConversationParticipants.GetParticipantAsync(conversationId, userId);
            return participant != null;
        }
    }
}