import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportService } from '../../../../services/report.service';
import { UserService } from '../../../../services/user.service';
import { OrganizationService } from '../../../../services/organization.service';

@Component({
  selector: 'app-sms-reports',
  templateUrl: './sms-reports.component.html',
  styleUrls: ['./sms-reports.component.scss']
})
export class SmsReportsComponent implements OnInit {
  filterForm: FormGroup;
  isLoading = false;
  reports: any[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  
  // Summary statistics
  totalMessages = 0;
  sentMessages = 0;
  deliveredMessages = 0;
  failedMessages = 0;
  pendingMessages = 0;
  totalRevenue = 0;
  deliveryRate = 0;
  
  
  // Filter options
  statusOptions = [
    { value: '', label: 'All Status' },
    { value: 'sent', label: 'Sent' },
    { value: 'delivered', label: 'Delivered' },
    { value: 'failed', label: 'Failed' },
    { value: 'pending', label: 'Pending' },
    { value: 'approved', label: 'Approved' },
    { value: 'rejected', label: 'Rejected' }
  ];

  organizationOptions: any[] = [];
  messageGroupOptions: any[] = [];

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private userService: UserService,
    private organizationService: OrganizationService
  ) {
    this.filterForm = this.fb.group({
      status: [''],
      startDate: [''],
      endDate: [''],
      organization: [''],
      messageGroup: [''],
      phoneNumber: [''],
      language: [''],
      content: ['']
    });
  }

  ngOnInit(): void {
    this.loadOrganizations();
    this.loadMessageGroups();
    this.loadSmsReports();
  }

  loadOrganizations(): void {
    this.organizationService.getOrganizations().subscribe({
      next: (organizations: any[]) => {
        this.organizationOptions = organizations.map(org => ({
          value: org.id,
          label: org.name
        }));
      },
      error: (error) => {
        console.error('Error loading organizations:', error);
      }
    });
  }

  loadMessageGroups(): void {
    const currentUser = this.userService.getCurrentUser();
    if (currentUser) {
      this.reportService.getReport('').subscribe({
        next: (messageGroups: any[]) => {
          this.messageGroupOptions = messageGroups.map(group => ({
            value: group.id,
            label: group.groupName
          }));
        },
        error: (error) => {
          console.error('Error loading message groups:', error);
        }
      });
    }
  }

  loadSmsReports(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;
    
    this.reportService.getSmsReports(filters).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.reports = response.data.reports || [];
          this.totalRecords = response.data.totalRecords || 0;
          this.calculateSummaryStatistics();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading SMS reports:', error);
        this.isLoading = false;
      }
    });
  }

  private calculateSummaryStatistics(): void {
    this.totalMessages = this.reports.length;
    this.sentMessages = this.reports.filter(r => r.messageStatus === 'sent').length;
    this.deliveredMessages = this.reports.filter(r => r.messageStatus === 'delivered').length;
    this.failedMessages = this.reports.filter(r => r.messageStatus === 'failed').length;
    this.pendingMessages = this.reports.filter(r => r.messageStatus === 'pending').length;
    
    // Calculate revenue (assuming 0.5 ETB per SMS)
    this.totalRevenue = this.deliveredMessages * 0.5;
    
    // Calculate delivery rate
    this.deliveryRate = this.totalMessages > 0 ? (this.deliveredMessages / this.totalMessages) * 100 : 0;
  }


  applyFilters(): void {
    this.currentPage = 1;
    this.loadSmsReports();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadSmsReports();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadSmsReports();
  }

  exportReports(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;
    
    this.reportService.exportSmsReports(filters).subscribe({
      next: (response: Blob) => {
        const blob = new Blob([response], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `sms-reports-${new Date().toISOString().split('T')[0]}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error exporting SMS reports:', error);
        alert('Error exporting reports. Please try again.');
        this.isLoading = false;
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered':
        return 'badge-success';
      case 'sent':
        return 'badge-primary';
      case 'failed':
        return 'badge-danger';
      case 'pending':
        return 'badge-warning';
      case 'approved':
        return 'badge-info';
      case 'rejected':
        return 'badge-secondary';
      default:
        return 'badge-secondary';
    }
  }

  getLanguageBadgeClass(language: string): string {
    switch (language.toLowerCase()) {
      case 'english':
        return 'badge-primary';
      case 'amharic':
        return 'badge-success';
      default:
        return 'badge-secondary';
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const startPage = Math.max(1, this.currentPage - 2);
    const endPage = Math.min(totalPages, this.currentPage + 2);
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-ET', {
      style: 'currency',
      currency: 'ETB'
    }).format(amount);
  }
}
