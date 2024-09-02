import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { AuthHeaderIneterceptor } from "./auth-header-interceptor";

export const httpInterceptProviders = [
    {
        provide :HTTP_INTERCEPTORS,useClass:AuthHeaderIneterceptor, multi :true
    }
]