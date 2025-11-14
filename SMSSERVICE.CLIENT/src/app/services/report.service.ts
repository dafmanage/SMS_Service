import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { IReportGetDto } from 'src/models/report/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getReport(id:string) {
    return this.http.get<IReportGetDto[]>(this.baseUrl + "/Report?messageGroupId=" + id)
  }

  // Delivery Reports Methods
  getDeliveryReports(filters: any) {
    return this.http.get(`${this.baseUrl}/Report/delivery-reports`, { params: filters });
  }

  exportDeliveryReports(filters: any) {
    return this.http.post(`${this.baseUrl}/Report/export-delivery-reports`, filters, { responseType: 'blob' });
  }

  // SMS Reports Methods
  getSmsReports(filters: any) {
    return this.http.get(`${this.baseUrl}/Report/sms-reports`, { params: filters });
  }

  exportSmsReports(filters: any) {
    return this.http.post(`${this.baseUrl}/Report/export-sms-reports`, filters, { responseType: 'blob' });
  }

  getSmsStatistics(filters: any) {
    return this.http.get(`${this.baseUrl}/Report/sms-statistics`, { params: filters });
  }
}
