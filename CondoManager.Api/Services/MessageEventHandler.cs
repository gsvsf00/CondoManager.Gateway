using CondoManager.Entity.Events;
using CondoManager.Entity.Models;
using CondoManager.Entity.Enums;
using CondoManager.Repository.Interfaces;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Services.Interfaces;

namespace CondoManager.Api.Services
{
    public class MessageEventHandler : IMessageEventHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RabbitMqPublisher _eventPublisher;
        private readonly ILogger<MessageEventHandler> _logger;

        public MessageEventHandler(
            IUnitOfWork unitOfWork,
            RabbitMqPublisher eventPublisher,
            ILogger<MessageEventHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task HandleMessageReceivedAsync(MessageReceivedEvent messageEvent)
        {
            try
            {
                Conversation? conversation = null;

                // Handle direct messages
                if (messageEvent.ChatType == ChatType.Direct && messageEvent.RecipientId.HasValue)
                {
                    // Find or create direct conversation
                    conversation = await _unitOfWork.Conversations.GetDirectConversationAsync(
                        messageEvent.SenderId, messageEvent.RecipientId.Value);

                    if (conversation == null)
                    {
                        conversation = new Conversation
                        {
                            Type = ConversationType.Direct,
                            CreatedByUserId = messageEvent.SenderId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        await _unitOfWork.Conversations.AddAsync(conversation);

                        // Add participants
                        await _unitOfWork.ConversationParticipants.AddAsync(new ConversationParticipant
                        {
                            ConversationId = conversation.Id,
                            UserId = messageEvent.SenderId,
                            JoinedAt = DateTime.UtcNow,
                            IsActive = true
                        });

                        await _unitOfWork.ConversationParticipants.AddAsync(new ConversationParticipant
                        {
                            ConversationId = conversation.Id,
                            UserId = messageEvent.RecipientId.Value,
                            JoinedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }
                // Handle group messages
                else if (messageEvent.ChatType == ChatType.ApartmentGroup && messageEvent.ConversationId.HasValue)
                {
                    conversation = await _unitOfWork.Conversations.GetByIdAsync(messageEvent.ConversationId.Value);
                    if (conversation == null)
                    {
                        _logger.LogWarning($"Conversation {messageEvent.ConversationId} not found for group message");
                        return;
                    }

                    // Verify user is participant
                    var isParticipant = await _unitOfWork.ConversationParticipants
                        .IsUserParticipantAsync(conversation.Id, messageEvent.SenderId);
                    if (!isParticipant)
                    {
                        _logger.LogWarning($"User {messageEvent.SenderId} is not a participant in conversation {conversation.Id}");
                        return;
                    }
                }

                if (conversation == null)
                {
                    _logger.LogError("Could not determine conversation for message");
                    return;
                }

                // Create and save message
                var message = new Message
                {
                    SenderId = messageEvent.SenderId,
                    ConversationId = conversation.Id,
                    RecipientId = messageEvent.RecipientId,
                    ChatType = messageEvent.ChatType,
                    Type = messageEvent.MessageType,
                    Content = messageEvent.Content,
                    SentAt = messageEvent.ReceivedAt,
                    IsAnnouncement = messageEvent.IsAnnouncement
                };

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.Conversations.UpdateLastMessageTimeAsync(conversation.Id, messageEvent.ReceivedAt);
                await _unitOfWork.SaveChangesAsync();

                // Publish message saved event back to CommunicationService
                var savedEvent = new MessageSavedEvent
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    RecipientId = message.RecipientId,
                    ConversationId = message.ConversationId,
                    Content = message.Content,
                    MessageType = message.Type,
                    ChatType = message.ChatType,
                    SentAt = message.SentAt,
                    IsAnnouncement = message.IsAnnouncement
                };

                await _eventPublisher.PublishAsync(savedEvent, "message.saved");

                _logger.LogInformation($"Message {message.Id} saved successfully for conversation {conversation.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message received event");
                throw;
            }
        }

        public async Task HandleMessageReadAsync(MessageReadEvent readEvent)
        {
            try
            {
                var message = await _unitOfWork.Messages.GetByIdAsync(readEvent.MessageId);
                if (message == null)
                {
                    _logger.LogWarning($"Message {readEvent.MessageId} not found for read event");
                    return;
                }

                // Update message read status
                message.IsRead = true;
                message.ReadAt = readEvent.ReadAt;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Message {readEvent.MessageId} marked as read by user {readEvent.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling message read event for message {readEvent.MessageId}");
                throw;
            }
        }

        public async Task HandleMessageDeliveredAsync(MessageDeliveredEvent deliveredEvent)
        {
            try
            {
                var message = await _unitOfWork.Messages.GetByIdAsync(deliveredEvent.MessageId);
                if (message == null)
                {
                    _logger.LogWarning($"Message {deliveredEvent.MessageId} not found for delivered event");
                    return;
                }

                // Update message delivery status
                message.IsDelivered = true;
                message.DeliveredAt = deliveredEvent.DeliveredAt;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Message {deliveredEvent.MessageId} marked as delivered to user {deliveredEvent.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling message delivered event for message {deliveredEvent.MessageId}");
                throw;
            }
        }
    }
}