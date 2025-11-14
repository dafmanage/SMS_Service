// Angular Import
import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

// project import
import { SharedModule } from 'src/app/theme/shared/shared.module';
import { UserService } from 'src/app/services/user.service';
import { ReportService } from 'src/app/services/report.service';

// Bootstrap Import
import { NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap';

// third party
import { NgApexchartsModule } from 'ng-apexcharts';
import ApexCharts from 'apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ChartComponent,
  ApexDataLabels,
  ApexPlotOptions,
  ApexResponsive,
  ApexXAxis,
  ApexGrid,
  ApexStroke,
  ApexTooltip,
  ApexYAxis,
  ApexLegend
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  dataLabels: ApexDataLabels;
  plotOptions: ApexPlotOptions;
  responsive: ApexResponsive[];
  xaxis: ApexXAxis;
  yaxis?: ApexYAxis;
  colors: string[];
  grid: ApexGrid;
  tooltip: ApexTooltip;
  stroke: ApexStroke;
  legend?: ApexLegend;
  fill?: any; // Add fill property for gradient charts
};

@Component({
  selector: 'app-default',
  standalone: true,
  imports: [CommonModule, SharedModule, NgApexchartsModule],
  templateUrl: './default.component.html',
  styleUrls: ['./default.component.scss']
})
export default class DefaultComponent implements OnInit {
  // private props
  @ViewChild('smsVolumeChart') smsVolumeChart: ChartComponent;
  @ViewChild('deliveryRateChart') deliveryRateChart: ChartComponent;
  @ViewChild('revenueChart') revenueChart: ChartComponent;
  
  constructor(
    private userService: UserService,
    private reportService: ReportService
  ) {
    this.initializeCharts();
  }
  
  ngOnInit(): void {
    this.loadDashboardData();
    // Initialize charts after view is ready
    setTimeout(() => {
      this.renderCharts();
    }, 500);
  }
  
  chartOptions: Partial<ChartOptions>;
  deliveryRateOptions: Partial<ChartOptions>;
  revenueOptions: Partial<ChartOptions>;
  
  // SMS Service Metrics
  smsMetrics = {
    totalSent: 2456789,
    totalDelivered: 2345678,
    totalFailed: 111111,
    deliveryRate: 95.5,
    pendingMessages: 45678,
    totalRevenue: 1250000,
    monthlyGrowth: 12.5
  };

  // Recent SMS Activities
  recentActivities = [
    {
      organization: 'Ethio Bank',
      messageType: 'OTP',
      status: 'Delivered',
      timestamp: '2 min ago',
      icon: 'ti ti-check-circle',
      color: 'text-success'
    },
    {
      organization: 'Tele Birr',
      messageType: 'Promotional',
      status: 'Pending',
      timestamp: '5 min ago',
      icon: 'ti ti-clock',
      color: 'text-warning'
    },
    {
      organization: 'Dashen Bank',
      messageType: 'Alert',
      status: 'Delivered',
      timestamp: '8 min ago',
      icon: 'ti ti-check-circle',
      color: 'text-success'
    },
    {
      organization: 'Commercial Bank',
      messageType: 'OTP',
      status: 'Failed',
      timestamp: '12 min ago',
      icon: 'ti ti-x-circle',
      color: 'text-danger'
    }
  ];

  // Top Organizations by SMS Volume
  topOrganizations = [
    {
      name: 'Ethio Bank',
      smsCount: 456789,
      growth: '+15.2%',
      status: 'active',
      icon: 'ti ti-building-bank'
    },
    {
      name: 'Tele Birr',
      smsCount: 345678,
      growth: '+8.7%',
      status: 'active',
      icon: 'ti ti-device-mobile'
    },
    {
      name: 'Dashen Bank',
      smsCount: 234567,
      growth: '+12.1%',
      status: 'active',
      icon: 'ti ti-building-bank'
    },
    {
      name: 'Commercial Bank',
      smsCount: 198765,
      growth: '+5.3%',
      status: 'active',
      icon: 'ti ti-building-bank'
    }
  ];


  private initializeCharts(): void {
    // SMS Volume Chart
    this.chartOptions = {
      series: [
        {
          name: 'SMS Sent',
          data: [45678, 52345, 48912, 56789, 61234, 57890, 65432, 59876, 67890, 71234, 68901, 74567]
        },
        {
          name: 'SMS Delivered',
          data: [43210, 49876, 46543, 54321, 58901, 55678, 62345, 56789, 64567, 67890, 65432, 71234]
        }
      ],
      chart: {
        type: 'area',
        height: 350,
        toolbar: {
          show: false
        },
        zoom: {
          enabled: false
        }
      },
      colors: ['#3b82f6', '#10b981'],
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: 'smooth',
        width: 3
      },
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.7,
          opacityTo: 0.2,
          stops: [0, 90, 100]
        }
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        labels: {
          style: {
            colors: '#6b7280'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#6b7280'
          },
          formatter: function(value) {
            return (value / 1000).toFixed(0) + 'K';
          }
        }
      },
      grid: {
        borderColor: '#e5e7eb',
        strokeDashArray: 4
      },
      tooltip: {
        theme: 'light',
        y: {
          formatter: function(value) {
            return value.toLocaleString() + ' SMS';
          }
        }
      },
      legend: {
        position: 'top',
        horizontalAlign: 'right'
      }
    };

    // Delivery Rate Chart
    this.deliveryRateOptions = {
      series: [95.5],
      chart: {
        type: 'radialBar',
        height: 200,
        offsetY: 0
      },
      colors: ['#10b981'],
      plotOptions: {
        radialBar: {
          startAngle: -135,
          endAngle: 135,
          hollow: {
            margin: 15,
            size: '70%'
          },
          track: {
            background: '#e5e7eb',
            strokeWidth: '97%',
            margin: 5
          },
          dataLabels: {
            name: {
              show: false
            },
            value: {
              fontSize: '30px',
              show: true,
              color: '#111',
              offsetY: 10,
              formatter: function(val: any) {
                return val + '%';
              }
            }
          }
        }
      },
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'dark',
          type: 'horizontal',
          shadeIntensity: 0.5,
          gradientToColors: ['#34d399'],
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 1,
          stops: [0, 100]
        }
      },
      stroke: {
        lineCap: 'round'
      }
    } as any; // Use type assertion to bypass type checking for now

    // Revenue Chart
    this.revenueOptions = {
      series: [
        {
          name: 'Revenue (ETB)',
          data: [125000, 138000, 145000, 156000, 162000, 178000, 185000, 192000, 198000, 205000, 218000, 225000]
        }
      ],
      chart: {
        type: 'line',
        height: 200,
        sparkline: {
          enabled: true
        }
      },
      colors: ['#f59e0b'],
      stroke: {
        curve: 'smooth',
        width: 3
      },
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.7,
          opacityTo: 0.2,
          stops: [0, 90, 100]
        }
      },
      tooltip: {
        theme: 'light',
        y: {
          formatter: function(value) {
            return 'ETB ' + (value / 1000).toFixed(0) + 'K';
          }
        }
      }
    };
  }

  private renderCharts(): void {
    // Charts will be rendered by the template
  }

  // Format numbers with Ethiopian currency
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-ET', {
      style: 'currency',
      currency: 'ETB',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  // Format large numbers
  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  // Get status color
  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered': return 'text-success';
      case 'pending': return 'text-warning';
      case 'failed': return 'text-danger';
      default: return 'text-muted';
    }
  }

  // Get status icon
  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered': return 'ti ti-check-circle';
      case 'pending': return 'ti ti-clock';
      case 'failed': return 'ti ti-x-circle';
      default: return 'ti ti-help-circle';
    }
  }

  // Load organization-specific dashboard data
  loadDashboardData(): void {
    const currentUser = this.userService.getCurrentUser();
    if (currentUser) {
      // Load organization-specific metrics
      this.loadOrganizationMetrics(currentUser.organizationId);
      this.loadRecentActivities(currentUser.organizationId);
    }
  }
  
  loadOrganizationMetrics(organizationId: string): void {
    // Load SMS metrics for the organization
    this.reportService.getReport(organizationId).subscribe({
      next: (response: any) => {
        if (response && response.length > 0) {
          this.updateSMSMetrics(response);
        }
      },
      error: (error) => {
        console.error('Error loading organization metrics:', error);
      }
    });
  }
  
  loadRecentActivities(organizationId: string): void {
    // Load recent activities for the organization
    // This would be implemented based on your specific requirements
    console.log('Loading recent activities for organization:', organizationId);
  }
  
  updateSMSMetrics(data: any[]): void {
    // Calculate metrics from the data
    const totalSent = data.reduce((sum, item) => sum + (item.numberOfCustomer || 0), 0);
    const deliveredCount = data.filter(item => item.messageStatus === 'Delivered').length;
    const pendingCount = data.filter(item => item.messageStatus === 'Pending').length;
    const failedCount = data.filter(item => item.messageStatus === 'Failed').length;
    
    this.smsMetrics = {
      totalSent: totalSent,
      totalDelivered: deliveredCount,
      totalFailed: failedCount,
      deliveryRate: totalSent > 0 ? (deliveredCount / totalSent) * 100 : 0,
      pendingMessages: pendingCount,
      totalRevenue: totalSent * 0.5, // Assuming 0.5 ETB per SMS
      monthlyGrowth: 12.5 // This would be calculated from historical data
    };
  }

  // Get current date for template
  getCurrentDate(): Date {
    return new Date();
  }
}
