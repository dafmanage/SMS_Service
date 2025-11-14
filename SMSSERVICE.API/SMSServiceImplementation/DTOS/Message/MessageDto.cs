using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceImplementation.DTOS.Message
{
    public class MessageGetDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string MessageStatus { get; set; }
        public string Language { get; set; }
        public string MessageGroup { get; set; }
        public Guid MessageGroupId { get; set; }
        public bool IsApproved { get; set; }
        public int TextSize { get; set; }
        public int NumberOfCustomer { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }



    }

    public class MessagePostDto
    {
        public string Content { get; set; }       
        public string Language { get; set; }
        public Guid MessageGroupId { get; set; }       
        public string CreatedById { get; set; }
        public Guid OrganizationId { get; set; }
    }

    public class MessageRejectDto
    {
        [Required(ErrorMessage = "Message ID is required")]
        public Guid MessageId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10 and 500 characters")]
        public string RejectionReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rejected by user ID is required")]
        public string RejectedById { get; set; } = string.Empty;

        public DateTime RejectedAt { get; set; } = DateTime.UtcNow;
    }

    public class MessageApproveDto
    {
        [Required(ErrorMessage = "Message ID is required")]
        public Guid MessageId { get; set; }

        [Required(ErrorMessage = "Approved by user ID is required")]
        public string ApprovedById { get; set; } = string.Empty;

        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

        [StringLength(200, ErrorMessage = "Approval notes cannot exceed 200 characters")]
        public string? ApprovalNotes { get; set; }
    }

    public class MessageStatusUpdateDto
    {
        [Required(ErrorMessage = "Message ID is required")]
        public Guid MessageId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Updated by user ID is required")]
        public string UpdatedById { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Status notes cannot exceed 500 characters")]
        public string? StatusNotes { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MessageDetailDto : MessageGetDto
    {
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
        public string? StatusNotes { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
