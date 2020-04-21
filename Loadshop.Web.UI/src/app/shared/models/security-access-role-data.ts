import { ISecurityAppActionData } from '.';

// eslint-disable-next-line @typescript-eslint/interface-name-prefix
export interface ISecurityAccessRoleData {
  accessRoleId: string;
  accessRoleName: string;
  accessRoleLevel: number;

  appActions: ISecurityAppActionData[];
}
