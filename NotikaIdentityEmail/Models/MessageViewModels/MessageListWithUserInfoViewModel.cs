namespace NotikaIdentityEmail.Models.MessageViewModels
{
    public class MessageListWithUserInfoViewModel
    {
        public string FullName { get; set; }
        public string FullProfileImageUrl { get; set; }
        public string MessageDetail { get; set; }
        public DateTime SendDate { get; set; }
    }
}
