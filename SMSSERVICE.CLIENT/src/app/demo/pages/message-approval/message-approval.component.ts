import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ReportService } from 'src/app/services/report.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-message-approval',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-approval.component.html',
  styleUrls: ['./message-approval.component.scss']
})
export class MessageApprovalComponent implements OnInit {
  pendingMessages: any[] = [];
  isLoading = false;
  approvalForm: FormGroup;
  selectedMessage: any = null;
  
  // Summary counts
  totalPending = 0;
  totalApproved = 0;
  totalRejected = 0;

  constructor(
    private reportService: ReportService,
    private userService: UserService,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.approvalForm = this.fb.group({
      approvalNotes: [''],
      rejectionReason: ['']
    });
  }

  ngOnInit(): void {
    this.loadPendingMessages();
  }

  loadPendingMessages(): void {
    this.isLoading = true;
    const currentUser = this.userService.getCurrentUser();
    
    if (currentUser && currentUser.role.includes('SuperAdmin')) {
      // SuperAdmin can see all pending messages
      this.reportService.getReport('').subscribe({
        next: (response: any) => {
          this.pendingMessages = response.filter((msg: any) => msg.messageStatus === 'Pending');
          this.calculateSummaryCounts();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading pending messages:', error);
          this.isLoading = false;
        }
      });
    } else {
      this.messageService.add({
        severity: 'warn',
        summary: 'Access Denied',
        detail: 'Only SuperAdmin can access message approval.'
      });
      this.isLoading = false;
    }
  }

  calculateSummaryCounts(): void {
    this.totalPending = this.pendingMessages.length;
    this.totalApproved = this.pendingMessages.filter(msg => msg.isApproved).length;
    this.totalRejected = this.pendingMessages.filter(msg => msg.messageStatus === 'Rejected').length;
  }

  approveMessage(message: any): void {
    this.selectedMessage = message;
    this.approvalForm.patchValue({
      approvalNotes: '',
      rejectionReason: ''
    });
    
    // Here you would call the API to approve the message
    this.messageService.add({
      severity: 'success',
      summary: 'Message Approved',
      detail: 'Message has been approved successfully.'
    });
    
    this.loadPendingMessages();
  }

  rejectMessage(message: any): void {
    this.selectedMessage = message;
    this.approvalForm.patchValue({
      approvalNotes: '',
      rejectionReason: ''
    });
    
    // Here you would call the API to reject the message
    this.messageService.add({
      severity: 'warn',
      summary: 'Message Rejected',
      detail: 'Message has been rejected.'
    });
    
    this.loadPendingMessages();
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'badge-warning';
      case 'approved':
        return 'badge-success';
      case 'rejected':
        return 'badge-danger';
      case 'sent':
        return 'badge-info';
      default:
        return 'badge-secondary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'ti ti-clock';
      case 'approved':
        return 'ti ti-check';
      case 'rejected':
        return 'ti ti-x';
      case 'sent':
        return 'ti ti-send';
      default:
        return 'ti ti-help';
    }
  }
}
