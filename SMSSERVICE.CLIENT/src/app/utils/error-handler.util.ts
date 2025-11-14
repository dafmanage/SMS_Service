export class ErrorHandlerUtil {
  static extractErrorMessage(error: any): string {
    console.log('Full error object:', error);
    
    // Handle validation errors from backend
    if (error.error && error.error.message) {
      console.log('Error message:', error.error.message);
      console.log('Error data:', error.error.data);
      
      if (error.error.data && Array.isArray(error.error.data)) {
        return error.error.data.join(', ');
      }
      return error.error.message;
    } 
    
    // Handle direct error message
    if (error.message) {
      console.log('Direct error message:', error.message);
      return error.message;
    }
    
    // Handle HTTP status codes
    if (error.status === 400) {
      return 'Validation failed. Please check your input.';
    } else if (error.status === 401) {
      return 'Unauthorized. Please login again.';
    } else if (error.status === 403) {
      return 'Access denied. Please contact your administrator.';
    } else if (error.status === 404) {
      return 'Resource not found.';
    } else if (error.status === 500) {
      return 'Server error. Please try again later.';
    } else if (error.status === 0) {
      return 'Unable to connect to server. Please check your internet connection.';
    }
    
    // Fallback
    return 'An unexpected error occurred';
  }
}
