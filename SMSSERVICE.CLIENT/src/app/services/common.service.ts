import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root',
})
export class CommonService {

  baseUrl: string = environment.baseUrl + '/common'
  baseUrlPdf : string = environment.baseUrl
  constructor( private http: HttpClient,private sanitizer: DomSanitizer) { }



  createImgPath = (dbPath: String) => {

    return `${environment.assetUrl}/${dbPath}`;
  }

  getDataDiff(startDat: string, endDat: string) {

    var startDate = new Date(startDat)
    var endDate = new Date(endDat)
    var diff = endDate.getTime() - startDate.getTime();
    var days = Math.floor(diff / (60 * 60 * 24 * 1000));
    var hours = Math.floor(diff / (60 * 60 * 1000)) - (days * 24);
    var minutes = Math.floor(diff / (60 * 1000)) - ((days * 24 * 60) + (hours * 60));
    var seconds = Math.floor(diff / 1000) - ((days * 24 * 60 * 60) + (hours * 60 * 60) + (minutes * 60));
    return { day: days, hour: hours, minute: minutes, second: seconds };
  }

  // getPdf (path : string ):any{

  //   var url = this.baseUrlPdf + "/pdf?path="+path

  //   return this.sanitizer.bypassSecurityTrustResourceUrl(url)
  // }


  getCurrentLocation() {
    return new Promise((resolve, reject) => {
      if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
          (position) => {
            if (position) {
              console.log(
                'Latitude: ' +
                position.coords.latitude +
                'Longitude: ' +
                position.coords.longitude
              );
              let lat = position.coords.latitude;
              let lng = position.coords.longitude;

              const location = {
                lat,
                lng,
              };
              resolve(location);
            }
          },
          (error) => console.log(error)
        );
      } else {
        reject('Geolocation is not supported by this browser.');
      }
    });
  }

  calculateAge(birthdate: Date): number {
    const today = new Date();
    const birthDate = new Date(birthdate);

    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    return age;
  }
  calculateDate(fromDate:Date,toDate:Date): number {
    const millisecondsPerDay = 24 * 60 * 60 * 1000; // Number of milliseconds in a day

    // Parse the date strings into Date objects
    const fromDateObj = new Date(fromDate);
    const toDateObj = new Date(toDate);

    // Convert the dates to UTC to handle potential timezone differences
    const fromTime = Date.UTC(fromDateObj.getUTCFullYear(), fromDateObj.getUTCMonth(), fromDateObj.getUTCDate());
    const toTime = Date.UTC(toDateObj.getUTCFullYear(), toDateObj.getUTCMonth(), toDateObj.getUTCDate());

    // Calculate the difference in milliseconds
    const differenceMs = Math.abs(toTime - fromTime);

    // Convert the difference to days
    const differenceDays = Math.floor(differenceMs / millisecondsPerDay);

    return differenceDays;
  }

  convertToDate(dateTimeString: string): string {
    const date = new Date(dateTimeString);
    const year = date.getFullYear();
    const month = ("0" + (date.getMonth() + 1)).slice(-2);
    const day = ("0" + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;

  }






}
