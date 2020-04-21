import { AuditType } from './audit-type';

export class LoadAudit {

  constructor(public loadId: string, public auditType: AuditType) {}
}

