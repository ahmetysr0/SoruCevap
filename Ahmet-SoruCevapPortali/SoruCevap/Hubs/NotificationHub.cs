using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    public async Task SendAnswerNotification(string questionOwnerId, string message, int questionId)
    {
        await Clients.User(questionOwnerId).SendAsync("ReceiveAnswerNotification", message, questionId);
    }

    public async Task SendNewAnswer(int questionId, string answer, string userName, string userId)
    {
        await Clients.Group($"Question_{questionId}").SendAsync("ReceiveNewAnswer", answer, userName, userId);
    }

    public async Task JoinQuestionGroup(string questionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Question_{questionId}");
    }

    public async Task LeaveQuestionGroup(string questionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Question_{questionId}");
    }
} 