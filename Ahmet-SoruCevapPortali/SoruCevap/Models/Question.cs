namespace SoruCevap.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Question
    {
        public Question()
        {
            Answers = new List<Answer>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public AppUser CreatedBy { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public QuestionStatus Status { get; set; }
        public string? ReviewedById { get; set; }
        public AppUser? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool IsResolved { get; set; }         // Soru çözüldü mü?
        public int ViewCount { get; set; }           // Görüntülenme sayısı
        public int VoteCount { get; set; }           // Oy sayısı
        public string Tags { get; set; }       // Etiketler
        public DateTime? LastActivityDate { get; set; }
        public bool IsFeatured { get; set; }         // Öne çıkan soru mu?
        public string? RejectionReason { get; set; } // Red edilme sebebi
        
        public ICollection<Answer> Answers { get; set; }
        public ICollection<QuestionVote> Votes { get; set; }
        public ICollection<QuestionReport> Reports { get; set; }

        // Tags için yardımcı property
        [NotMapped] // Bu property veritabanında bir kolon oluşturmayacak
        public List<string> TagList
        {
            get => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            set => Tags = value != null ? string.Join(',', value) : null;
        }
    }
    public enum QuestionStatus
    {
        Pending,    // Onay Bekliyor
        Approved,   // Onaylandı
        Rejected    // Reddedildi
    }
}
