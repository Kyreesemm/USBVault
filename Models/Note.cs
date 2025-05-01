using System;

namespace USBVault.Models
{
    public class Note
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime LastModifiedDate { get; private set; }

        public Note(string title, string text)
        {
            Title = title;
            Text = text;
            CreatedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
        }

        public void UpdateText(string newText)
        {
            Text = newText;
            LastModifiedDate = DateTime.Now;
        }
    }
}