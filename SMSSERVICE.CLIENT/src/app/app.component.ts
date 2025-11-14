import { Component, OnInit, OnDestroy } from '@angular/core';
import { SessionTimeoutService } from './services/session-timeout.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'DAFTech Bill System';

  constructor(private sessionTimeoutService: SessionTimeoutService) {}

  ngOnInit(): void {
    // Session timeout service is automatically initialized in its constructor
  }

  ngOnDestroy(): void {
    this.sessionTimeoutService.destroy();
  }
}
