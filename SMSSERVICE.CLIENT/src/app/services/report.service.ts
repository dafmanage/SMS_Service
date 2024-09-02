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
}
