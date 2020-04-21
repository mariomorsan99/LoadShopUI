import { HttpErrorResponse } from '@angular/common/http';
import { Action } from '@ngrx/store';
import { SpecialInstructionData } from '../../../shared/models/special-instruction-data';

export enum SpecialInstructionsActionTypes {
  Load = '[SpecialInstructions] LOAD',
  Load_Success = '[SpecialInstructions] LOAD_SUCCESS',
  Load_Failure = '[SpecialInstructions] LOAD_FAILURE',
  Load_Instruction = '[SpecialInstructions] LOAD_INSTRUCTION',
  Load_Instruction_Success = '[SpecialInstructions] LOAD_INSTRUCTION_SUCCESS',
  Load_Instruction_Failure = '[SpecialInstructions] LOAD_INSTRUCTION_FAILURE',
  Add = '[SpecialInstructions] ADD',
  Add_Success = '[SpecialInstructions] ADD_SUCCESS',
  Add_Failure = '[SpecialInstructions] ADD_FAILURE',
  Update = '[SpecialInstructions] UPDATE',
  Update_Success = '[SpecialInstructions] UPDATE_SUCCESS',
  Update_Failure = '[SpecialInstructions] UPDATE_FAILURE',
  Delete = '[SpecialInstructions] DELETE',
  Delete_Success = '[SpecialInstructions] DELETE_SUCCESS',
  Delete_Failure = '[SpecialInstructions] DELETE_FAILURE',
  Clear_Save_Succeeded = '[SpecialInstructions] CLEAR_SAVE_SUCCEEDED',
}

export class SpecialInstructionsLoadAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load;
  constructor(public payload: { customerId: string }) {}
}
export class SpecialInstructionsLoadSuccessAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load_Success;
  constructor(public payload: SpecialInstructionData[]) {}
}
export class SpecialInstructionsLoadFailureAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load_Failure;
  constructor(public payload: Error) {}
}

export class SpecialInstructionsLoadInstructionAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load_Instruction;
  constructor(public payload: { specialInstructionId: number }) {}
}

export class SpecialInstructionsLoadInstructionSuccessAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load_Instruction_Success;
  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsLoadInstructionFailureAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Load_Instruction_Failure;
  constructor(public payload: Error) {}
}

export class SpecialInstructionsAddAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Add;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsAddSuccessAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Add_Success;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsAddFailureAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Add_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class SpecialInstructionsUpdateAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Update;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsUpdateSuccessAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Update_Success;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsUpdateFailureAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Update_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class SpecialInstructionsDeleteAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Delete;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsDeleteSuccessAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Delete_Success;

  constructor(public payload: SpecialInstructionData) {}
}

export class SpecialInstructionsDeleteFailureAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Delete_Failure;

  constructor(public payload: Error) {}
}

export class SpecialInstructionsClearSaveSucceededAction implements Action {
  readonly type = SpecialInstructionsActionTypes.Clear_Save_Succeeded;

  constructor() {}
}

export type SpecialInstructionsActions =
  | SpecialInstructionsLoadAction
  | SpecialInstructionsLoadSuccessAction
  | SpecialInstructionsLoadFailureAction
  | SpecialInstructionsLoadInstructionAction
  | SpecialInstructionsLoadInstructionSuccessAction
  | SpecialInstructionsLoadInstructionFailureAction
  | SpecialInstructionsAddAction
  | SpecialInstructionsAddSuccessAction
  | SpecialInstructionsAddFailureAction
  | SpecialInstructionsUpdateAction
  | SpecialInstructionsUpdateSuccessAction
  | SpecialInstructionsUpdateFailureAction
  | SpecialInstructionsDeleteAction
  | SpecialInstructionsDeleteSuccessAction
  | SpecialInstructionsDeleteFailureAction
  | SpecialInstructionsClearSaveSucceededAction;
