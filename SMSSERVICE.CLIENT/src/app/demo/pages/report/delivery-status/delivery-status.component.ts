import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReportService } from '../../../../services/report.service';

@Component({
  selector: 'app-delivery-status',
  templateUrl: './delivery-status.component.html',
  styleUrls: ['./delivery-status.component.scss']
})
export class DeliveryStatusComponent implements OnInit {
  filterForm: FormGroup;
  isLoading = false;
  reports: any[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  statusFilter = '';
  dateRange = '';
  Math = Math; // Make Math available in template
  
  // Summary counts
  sentCount = 0;
  deliveredCount = 0;
  failedCount = 0;
  pendingCount = 0;

  statusOptions = [
    { value: '', label: 'All Status' },
    { value: 'sent', label: 'Sent' },
    { value: 'delivered', label: 'Delivered' },
    { value: 'failed', label: 'Failed' },
    { value: 'pending', label: 'Pending' }
  ];

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService
  ) {
    this.filterForm = this.fb.group({
      status: [''],
      startDate: [''],
      endDate: [''],
      organization: [''],
      phoneNumber: ['']
    });
  }

  ngOnInit(): void {
    this.loadDeliveryReports();
  }

  loadDeliveryReports(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;
    
    this.reportService.getDeliveryReports(filters).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.reports = response.data.reports || [];
          this.totalRecords = response.data.totalRecords || 0;
          this.calculateSummaryCounts();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading delivery reports:', error);
        this.isLoading = false;
      }
    });
  }

  private calculateSummaryCounts(): void {
    this.sentCount = this.reports.filter(r => r.status === 'sent').length;
    this.deliveredCount = this.reports.filter(r => r.status === 'delivered').length;
    this.failedCount = this.reports.filter(r => r.status === 'failed').length;
    this.pendingCount = this.reports.filter(r => r.status === 'pending').length;
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadDeliveryReports();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadDeliveryReports();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDeliveryReports();
  }

  exportReports(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;
    
    this.reportService.exportDeliveryReports(filters).subscribe({
      next: (response: Blob) => {
        // Create download link
        const blob = new Blob([response], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `delivery-reports-${new Date().toISOString().split('T')[0]}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error exporting reports:', error);
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
}
