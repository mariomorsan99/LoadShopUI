import { Injectable } from '@angular/core';

import { MessageService } from 'primeng/components/common/messageservice';
import { ErrorHandler } from '@angular/core';

@Injectable()
export class ErrorService implements ErrorHandler {
  constructor(private messageService: MessageService) { }

  handleError(error: any): any {
    this.messageService.add({ severity: 'error', detail: error });
    console.error(error);
    return error;
  }
}

