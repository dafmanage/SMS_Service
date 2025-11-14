import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class BrowserSecurityService {

  constructor(private router: Router) {}

  /**
   * Prevents back button bypass by clearing browser history
   * and adding cache control headers
   */
  preventBackButtonBypass(): void {
    // Clear browser history
    this.clearBrowserHistory();
    
    // Add cache control headers
    this.addCacheControlHeaders();
    
    // Add event listeners to prevent back navigation
    this.addBackButtonPrevention();
  }

  /**
   * Clears browser history by replacing current state
   */
  private clearBrowserHistory(): void {
    // Replace current history state to prevent back navigation
    window.history.replaceState(null, '', window.location.href);
    
    // Clear any cached data
    if ('caches' in window) {
      caches.keys().then(names => {
        names.forEach(name => {
          caches.delete(name);
        });
      });
    }
  }

  /**
   * Adds cache control meta tags to prevent caching
   */
  private addCacheControlHeaders(): void {
    const metaTags = [
      { name: 'Cache-Control', content: 'no-cache, no-store, must-revalidate' },
      { name: 'Pragma', content: 'no-cache' },
      { name: 'Expires', content: '0' }
      // Removed Clear-Site-Data as it clears sessionStorage
    ];

    metaTags.forEach(tag => {
      this.addOrUpdateMetaTag(tag.name, tag.content);
    });
  }

  /**
   * Adds or updates a meta tag
   */
  private addOrUpdateMetaTag(name: string, content: string): void {
    // Remove existing meta tag if it exists
    const existingTag = document.querySelector(`meta[name="${name}"]`);
    if (existingTag) {
      existingTag.remove();
    }
    
    // Add new meta tag
    const meta = document.createElement('meta');
    meta.name = name;
    meta.content = content;
    document.head.appendChild(meta);
  }

  /**
   * Adds event listeners to prevent back button navigation
   */
  private addBackButtonPrevention(): void {
    // Listen for popstate events (back/forward button)
    window.addEventListener('popstate', (event) => {
      // Check if user has valid token
      const token = sessionStorage.getItem('token');
      if (!token || token.trim() === '') {
        // Redirect to login if no token
        this.router.navigate(['/auth/login'], { replaceUrl: true });
      }
    });

    // Listen for beforeunload to clear sensitive data
    window.addEventListener('beforeunload', () => {
      // Clear sensitive data on page unload
      sessionStorage.removeItem('token');
    });
  }

  /**
   * Securely redirects to login page with cache busting
   */
  secureRedirectToLogin(): void {
    // Clear all session data
    sessionStorage.clear();
    
    // Clear browser cache
    if ('caches' in window) {
      caches.keys().then(names => {
        names.forEach(name => {
          caches.delete(name);
        });
      });
    }
    
    // Redirect with cache busting parameter
    this.router.navigate(['/auth/login'], { 
      queryParams: { t: Date.now() },
      replaceUrl: true 
    });
  }

  /**
   * Adds security headers to HTTP responses (if possible)
   */
  addSecurityHeaders(): void {
    // Add security-related meta tags
    const securityTags = [
      { name: 'X-Frame-Options', content: 'DENY' },
      { name: 'X-Content-Type-Options', content: 'nosniff' },
      { name: 'X-XSS-Protection', content: '1; mode=block' },
      { name: 'Referrer-Policy', content: 'strict-origin-when-cross-origin' }
    ];

    securityTags.forEach(tag => {
      this.addOrUpdateMetaTag(tag.name, tag.content);
    });
  }
}
