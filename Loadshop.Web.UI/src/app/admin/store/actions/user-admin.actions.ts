/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { CarrierScac, Customer, ISecurityAccessRoleData, UserAdminData, Carrier } from '../../../shared/models';

export enum UserAdminActionTypes {
  Load_Users = '[UserAdmin] LOAD_USERS',
  Load_Users_Success = '[UserAdmin] LOAD_USERS_SUCCESS',
  Load_Users_Failure = '[UserAdmin] LOAD_USERS_FAILURE',
  Load_AdminUsers = '[UserAdmin] LOAD_ADMINUSERS',
  Load_AdminUsers_Success = '[UserAdmin] LOAD_ADMINUSERS_SUCCESS',
  Load_AdminUsers_Failure = '[UserAdmin] LOAD_ADMINUSERS_FAILURE',
  Load_User = '[UserAdmin] LOAD_USER',
  Load_User_Success = '[UserAdmin] LOAD_USER_SUCCESS',
  Load_User_Failure = '[UserAdmin] LOAD_USER_FAILURE',
  Load_Identity_User = '[UserAdmin] LOAD_IDENTITY_USER',
  Load_Identity_User_Success = '[UserAdmin] LOAD_IDENTITY_USER_SUCCESS',
  Load_Identity_User_Not_Found = '[UserAdmin] LOAD_IDENTITY_USER_NOT_FOUND',
  Load_Identity_User_Failure = '[UserAdmin] LOAD_IDENTITY_USER_Failure',
  Load_Authorized_Shippers = '[UserAdmin] LOAD_AUTHORIZED_SHIPPERS',
  Load_Authorized_Shippers_Success = '[UserAdmin] LOAD_AUTHORIZED_SHIPPERS_SUCCESS',
  Load_Authorized_Shippers_Failure = '[UserAdmin] LOAD_AUTHORIZED_SHIPPERS_FAILURE',
  Load_Authorized_CarrierScacs = '[UserAdmin] LOAD_AUTHORIZED_CARRIERSCACS',
  Load_Authorized_CarrierScacs_Success = '[UserAdmin] LOAD_AUTHORIZED_CARRIERSCACS_SUCCESS',
  Load_Authorized_CarrierScacs_Failure = '[UserAdmin] LOAD_AUTHORIZED_CARRIERSCACS_FAILURE',
  Load_Authorized_Security_Roles = '[UserAdmin] LOAD_AUTHORIZED_SECURITY_ROLES',
  Load_Authorized_Security_Roles_Success = '[UserAdmin] LOAD_AUTHORIZED_SECURITY_ROLES_SUCCESS',
  Load_Authorized_Security_Roles_Failure = '[UserAdmin] LOAD_AUTHORIZED_SECURITY_ROLES_FAILURE',
  Update_User = '[UserAdmin] UPDATE_USER',
  Update_User_Success = '[UserAdmin] UPDATE_USER_SUCCESS',
  Update_User_Failure = '[UserAdmin] UPDATE_USER_FAILURE',
  Create_User = '[UserAdmin] CREATE_USER',
  Create_User_Success = '[UserAdmin] CREATE_USER_SUCCESS',
  Create_User_Failure = '[UserAdmin] CREATE_USER_FAILURE',
  Load_Authorized_Carriers = '[UserAdmin] LOAD_AUTHORIZED_CARRIERS',
  Load_Authorized_Carriers_Success = '[UserAdmin] LOAD_AUTHORIZED_CARRIERS_SUCCESS',
  Load_Authorized_Carriers_Failure = '[UserAdmin] LOAD_AUTHORIZED_CARRIERS_FAILURE',
  Load_Carrier_Users = '[UserAdmin] LOAD_CARRIER_USERS',
  Load_Carrier_Users_Success = '[UserAdmin] LOAD_CARRIER_USERS_SUCCESS',
  Load_Carrier_Users_Failure = '[UserAdmin] LOAD_CARRIER_USERS_FAILURE',
  Clear_Selected_User = '[UserAdmin] CLEAR_SELECTED_USER'
}

export class UserAdminLoadUsersAction implements Action {
  readonly type = UserAdminActionTypes.Load_Users;
  constructor(public payload: { query: string }) {}
}

export class UserAdminLoadUsersSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Users_Success;

  constructor(public payload: UserAdminData[]) {}
}

export class UserAdminLoadUsersFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Users_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadAdminUsersAction implements Action {
  readonly type = UserAdminActionTypes.Load_AdminUsers;
}

export class UserAdminLoadAdminUsersSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_AdminUsers_Success;

  constructor(public payload: UserAdminData[]) {}
}

export class UserAdminLoadAdminUsersFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_AdminUsers_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadUserAction implements Action {
  readonly type = UserAdminActionTypes.Load_User;
  constructor(public payload: { userId: string }) {}
}

export class UserAdminLoadUserSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_User_Success;

  constructor(public payload: UserAdminData) {}
}

export class UserAdminClearSelectedUserAction implements Action {
  readonly type = UserAdminActionTypes.Clear_Selected_User;
  constructor() {}
}

export class UserAdminLoadUserFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_User_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadIdentityUserAction implements Action {
  readonly type = UserAdminActionTypes.Load_Identity_User;
  constructor(public payload: { username: string }) {}
}

export class UserAdminLoadIdentityUserSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Identity_User_Success;

  constructor(public payload: UserAdminData) {}
}

export class UserAdminLoadIdentityUserNotFoundAction implements Action {
  readonly type = UserAdminActionTypes.Load_Identity_User_Not_Found;
}

export class UserAdminLoadIdentityUserFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Identity_User_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadAuthorizedShippersAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Shippers;
}

export class UserAdminLoadAuthorizedShippersSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Shippers_Success;

  constructor(public payload: Customer[]) {}
}

export class UserAdminLoadAuthorizedShippersFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Shippers_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadAuthorizedCarrierScacsAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_CarrierScacs;
}

export class UserAdminLoadAuthorizedCarrierScacsSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_CarrierScacs_Success;

  constructor(public payload: CarrierScac[]) {}
}

export class UserAdminLoadAuthorizedCarrierScacsFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_CarrierScacs_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadAuthorizedSecurityRolesAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Security_Roles;
}

export class UserAdminLoadAuthorizedSecurityRolesSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Security_Roles_Success;

  constructor(public payload: ISecurityAccessRoleData[]) {}
}

export class UserAdminLoadAuthorizedSecurityRolesFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Security_Roles_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminUpdateUserAction implements Action {
  readonly type = UserAdminActionTypes.Update_User;
  constructor(public payload: { user: UserAdminData }) {}
}

export class UserAdminUpdateUserSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Update_User_Success;

  constructor(public payload: UserAdminData) {}
}

export class UserAdminUpdateUserFailureAction implements Action {
  readonly type = UserAdminActionTypes.Update_User_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminCreateUserAction implements Action {
  readonly type = UserAdminActionTypes.Create_User;
  constructor(public payload: { user: UserAdminData }) {}
}

export class UserAdminCreateUserSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Create_User_Success;

  constructor(public payload: UserAdminData) {}
}

export class UserAdminCreateUserFailureAction implements Action {
  readonly type = UserAdminActionTypes.Create_User_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadAuthorizedCarriersAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Carriers;
}

export class UserAdminLoadAuthorizedCarriersSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Carriers_Success;

  constructor(public payload: Carrier[]) {}
}

export class UserAdminLoadAuthorizedCarriersFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Authorized_Carriers_Failure;

  constructor(public payload: Error) {}
}

export class UserAdminLoadCarrierUsersAction implements Action {
  readonly type = UserAdminActionTypes.Load_Carrier_Users;
  constructor(public payload: string) {}
}

export class UserAdminLoadCarrierUsersSuccessAction implements Action {
  readonly type = UserAdminActionTypes.Load_Carrier_Users_Success;

  constructor(public payload: UserAdminData[]) {}
}

export class UserAdminLoadCarrierUsersFailureAction implements Action {
  readonly type = UserAdminActionTypes.Load_Carrier_Users_Failure;

  constructor(public payload: Error) {}
}

export type UserAdminActions =
  | UserAdminLoadUsersAction
  | UserAdminLoadUsersSuccessAction
  | UserAdminLoadUsersFailureAction
  | UserAdminLoadAdminUsersAction
  | UserAdminLoadAdminUsersSuccessAction
  | UserAdminLoadAdminUsersFailureAction
  | UserAdminLoadUserAction
  | UserAdminLoadUserSuccessAction
  | UserAdminLoadUserFailureAction
  | UserAdminLoadIdentityUserAction
  | UserAdminLoadIdentityUserSuccessAction
  | UserAdminLoadIdentityUserNotFoundAction
  | UserAdminLoadIdentityUserFailureAction
  | UserAdminLoadAuthorizedShippersAction
  | UserAdminLoadAuthorizedShippersSuccessAction
  | UserAdminLoadAuthorizedShippersFailureAction
  | UserAdminLoadAuthorizedCarrierScacsAction
  | UserAdminLoadAuthorizedCarrierScacsSuccessAction
  | UserAdminLoadAuthorizedCarrierScacsFailureAction
  | UserAdminLoadAuthorizedSecurityRolesAction
  | UserAdminLoadAuthorizedSecurityRolesSuccessAction
  | UserAdminLoadAuthorizedSecurityRolesFailureAction
  | UserAdminUpdateUserAction
  | UserAdminUpdateUserSuccessAction
  | UserAdminUpdateUserFailureAction
  | UserAdminCreateUserAction
  | UserAdminCreateUserSuccessAction
  | UserAdminCreateUserFailureAction
  | UserAdminLoadAuthorizedCarriersAction
  | UserAdminLoadAuthorizedCarriersSuccessAction
  | UserAdminLoadAuthorizedCarriersFailureAction
  | UserAdminLoadCarrierUsersAction
  | UserAdminLoadCarrierUsersSuccessAction
  | UserAdminLoadCarrierUsersFailureAction
  | UserAdminClearSelectedUserAction;
