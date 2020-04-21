import {
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';

import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { ResponseError } from '../../shared/models';
import { MessageService } from 'primeng/components/common/messageservice';

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {
  constructor(private messageService: MessageService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      map(x => {
        if (x instanceof HttpResponse) {
          if (x.body.errors && x.body.errors.length) {
            const errors: ResponseError[] = x.body.errors;
            console.error(errors);
            const msgs = errors.map(y => {
              return {
                detail: y.message,
                severity: 'error'
              };
            });
            this.messageService.addAll(msgs);
          }
        }
        return x;
      })
    );
  }
}
