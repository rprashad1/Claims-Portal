using System;
using System.Collections.Generic;

namespace ClaimsPortal.Data
{
    public class LetterGenFormData
    {
        public long Id { get; set; }
        public System.Guid GeneratedDocumentId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
