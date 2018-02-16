import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { functions } from '../func/fetchHelper';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface FileState {
    fileList: File[] | null,
    fileListCountView: number,
    fileListPage: number,
    needGetData: boolean,
    trackerId?: number,
    fileFilter?: string,
    userName?: string,
}
export interface File {
    FileId: number,
    FileName: string,
    FullName: string,
    FilePath: string,
    IsWasDeletedChange: boolean,
    RemoveFromDbTime?: Date,
    IsNeedDelete: boolean,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.
interface GetFileListAction {
    type: 'GET_FILE_LIST',
}
interface SetFileListAction {
    type: 'SET_FILE_LIST',
    fileList: File[] | null,
}
interface MovePageFileListAction {
    type: 'MOVE_PAGE_FILE_LIST_ACTION',
    prevOrNext: number,
}
interface SaveTrackerIdAction {
    type: 'SAVE_TRACKER_ID_ACTION',
    trackerId: number,
}
interface ResetFileListAction {
    type: 'RESET_FILE_LIST_ACTION',
}
interface ViewCountFileListAction {
    type: 'VIEW_COUNT_FILE_LIST_ACTION',
    count: number,
}
interface SetUserNameAction {
    type: 'SET_USER_NAME_ACTION',
    userName: string,
}
interface SetFileFilterAction {
    type: 'SET_FILE_FILTER_ACTION',
    fileFilter: string,
}
interface EditDeleteTimeAction {
    type: 'EDIT_DELETE_TIME_ACTION',
}
interface DeleteFileFilterAction {
    type: 'DELETE_FILE_FILTER_ACTION',
}
interface DeleteUserNameAction {
    type: 'DELETE_USER_NAME_ACTION',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).

type KnownAction = GetFileListAction | SetFileListAction | MovePageFileListAction | ResetFileListAction | ViewCountFileListAction
    | SaveTrackerIdAction | SetUserNameAction | SetFileFilterAction | EditDeleteTimeAction | DeleteFileFilterAction | DeleteUserNameAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetFileList: (trackerid?: number, userName?: string, fileFilter?: string, count?: string | number, page?: string | number): AppThunkAction<GetFileListAction | SetFileListAction | SaveTrackerIdAction | SetUserNameAction> => (dispatch, getState) => {
        let params = "";
        if (trackerid != null) {
            params = `?trackerid=${trackerid}`;
            if(count == null || page == null){
                dispatch({ type: 'SAVE_TRACKER_ID_ACTION', trackerId: trackerid });
            }
        }
        if(userName != null){
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `userName=${userName}`;
            if(count == null || page == null){
                dispatch({ type: 'SET_USER_NAME_ACTION', userName:userName });
            }
        }
        if (fileFilter != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `filter=${fileFilter}`;
        }
        if (count != null && page != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `count=${count}&page=${page}`;
        }
        let fetchTask = functions.fetchTask('GetFileList', 'GET', params)
            .then(data => {
                data = data as File[] | null;
                if(data){
                    data.forEach((item:File) => {
                        if(item.RemoveFromDbTime){
                            item.RemoveFromDbTime = new Date(item.RemoveFromDbTime);
                        }
                    });
                }
                dispatch({ type: 'SET_FILE_LIST', fileList: data as File[] | null });
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });

        addTask(fetchTask); 
        dispatch({ type: 'GET_FILE_LIST' });
    },
    EditDeleteTime: (fileId:number, dateTime: Date, isNeedDelete: boolean): AppThunkAction<EditDeleteTimeAction> => (dispatch, getState) => {
        debugger;
        let fetchTask = fetch(`/api/Admin/EditDeleteTime`, {
                method: 'PUT',
                body: JSON.stringify({fileId: fileId, dateTime: dateTime, isNeedDelete: isNeedDelete}),
                headers: new Headers({
                    'Content-Type': 'application/json'
                  })
            }).then(response => {
                if (response.status !== 200) return undefined;
                dispatch({ type: 'EDIT_DELETE_TIME_ACTION' });
                return response;
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });
        addTask(fetchTask); 
    },
    MovePageFileList: (prevOrNext: number) => <MovePageFileListAction>{ type: 'MOVE_PAGE_FILE_LIST_ACTION', prevOrNext: prevOrNext },
    ResetFileList: () => <ResetFileListAction>{ type: 'RESET_FILE_LIST_ACTION' },
    ViewCountFileList: (count: number) => <ViewCountFileListAction>{ type: 'VIEW_COUNT_FILE_LIST_ACTION', count: count },
    SetFileFilter:(fileFilter:string) => <SetFileFilterAction>{ type: 'SET_FILE_FILTER_ACTION', fileFilter:fileFilter },
    DeleteFileFilter:() => <DeleteFileFilterAction>{ type: 'DELETE_FILE_FILTER_ACTION' },
    DeleteUserName:() => <DeleteUserNameAction>{ type: 'DELETE_USER_NAME_ACTION' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: FileState = { fileList: null, fileListCountView: 10, fileListPage: 1, needGetData: false, fileFilter: undefined };

export const reducer: Reducer<FileState> = (state: FileState, action: KnownAction) => {
    switch (action.type) {
        case 'GET_FILE_LIST':
            return { 
                ...state,
                needGetData: false,
            };

        case 'SET_FILE_LIST':
            if (action.fileList == null) {
                var needEmptyList = state.fileFilter ? true : false;
                if(state.fileListPage == 1){
                    return{
                        ...state,
                        fileList: null,
                        needGetData: false,
                    }
                }
                let page = state.fileListPage < 1
                    ? 1
                    : state.fileListPage - 1;
                return {
                    ...state,
                    fileList: needEmptyList ? null : state.fileList,
                    fileListPage: page,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    fileList: action.fileList,
                    needGetData: false,
                };
            }

        case 'SAVE_TRACKER_ID_ACTION':
            return {
                ...state,
                trackerId: action.trackerId,
                fileListPage: 1,
            }

        case 'SET_USER_NAME_ACTION':
            return {
                ...state,
                userName: action.userName,
                fileListPage: 1,
            }

        case 'MOVE_PAGE_FILE_LIST_ACTION':
            let newPage = state.fileListPage + action.prevOrNext;
            if (newPage < 1) {
                return {
                    ...state,
                    fileListPage: 1,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    fileListPage: newPage,
                    needGetData: true,
                }
            }

        case 'RESET_FILE_LIST_ACTION':
            return unloadedState;

        case 'VIEW_COUNT_FILE_LIST_ACTION':
            return {
                ...state,
                fileListCountView: action.count,
                fileListPage: 1,
                needGetData: true,
            }

        case 'SET_FILE_FILTER_ACTION':
            return {
                ...state,
                fileFilter: action.fileFilter,
                fileListPage: 1,
                needGetData: true,
            }

        case 'EDIT_DELETE_TIME_ACTION':
            return {
                ...state,
                needGetData: true,
            }

        case 'DELETE_FILE_FILTER_ACTION':
            return {
                ...state,
                fileFilter: undefined,
                fileListPage: 1,
                needGetData: true,
            }

        case 'DELETE_USER_NAME_ACTION':
            return {
                ...state,
                userName: undefined,
                fileListPage: 1,
                needGetData: true,
            }

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};