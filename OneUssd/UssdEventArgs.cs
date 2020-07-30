using Android.Views.Accessibility;

namespace OneUssd
{
    public class UssdEventArgs
    {
        public UssdEventArgs(string message, AccessibilityEvent accessibilityEvent)
        {
            ResponseMessage = message;
            AccessibilityEvent = accessibilityEvent;
        }
        public UssdEventArgs(string message)
        {
            ResponseMessage = message;
        }
        public string ResponseMessage { get; }
        public AccessibilityEvent AccessibilityEvent { get; }
    }
}