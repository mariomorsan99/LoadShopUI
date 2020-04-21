import { Action } from '@ngrx/store';
import { Load } from '../../models';

export enum LoadBoardLoadBookActionTypes {
    Book = '[LoadBoardLoadBook] BOOK',
    Book_Success = '[LoadBoardLoadBook] BOOK_SUCCESS',
    Book_Failure = '[LoadBoardLoadBook] BOOK_FAILURE'
}

export class LoadBoardLoadBookAction implements Action {
    readonly type = LoadBoardLoadBookActionTypes.Book;

    constructor(public payload: Load) { }
}

export class LoadBoardLoadBookSuccessAction implements Action {
    readonly type = LoadBoardLoadBookActionTypes.Book_Success;

    constructor(public payload: Load) { }
}

export class LoadBoardLoadBookFailureAction implements Action {
    readonly type = LoadBoardLoadBookActionTypes.Book_Failure;

    constructor(public payload: Error) { }
}

export type LoadBoardLoadBookActions =
    LoadBoardLoadBookAction |
    LoadBoardLoadBookSuccessAction |
    LoadBoardLoadBookFailureAction;
