// Step 1: Define the delegate and event
public class BookingManager
{
    public delegate void BookingCompletedHandler(string message);
    public event BookingCompletedHandler BookingCompleted;

    public void BookWorkspace(int workspaceId)
    {
        // Booking logic
        Console.WriteLine($"Workspace {workspaceId} booked.");
        
        // Raise the event
        BookingCompleted?.Invoke($"Workspace {workspaceId} booking confirmed.");
    }
}

// Step 2: Define notification handlers (subscribers)
public class NotificationService
{
    public void SendEmail(string message)
    {
        Console.WriteLine($"Email Sent: {message}");
    }

    public void SendSMS(string message)
    {
        Console.WriteLine($"SMS Sent: {message}");
    }
}

// Step 3: Subscribe to the event
static class Booking
{
    static void Book()
    {
        BookingManager bookingManager = new BookingManager();
        NotificationService notificationService = new NotificationService();

        // Subscribe methods to the event
        bookingManager.BookingCompleted += notificationService.SendEmail;
        bookingManager.BookingCompleted += notificationService.SendSMS;

        // Trigger the event by booking a workspace
        bookingManager.BookWorkspace(101);
    }
}
