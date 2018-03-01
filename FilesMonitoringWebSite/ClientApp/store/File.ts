import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { functions } from '../func/RequestHelper';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface FileState {
    fileList?: File[],
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
interface ViewCountFileListAction {
    type: 'VIEW_COUNT_FILE_LIST_ACTION',
    count: number,
}
interface EditDeleteTimeAction {
    type: 'EDIT_DELETE_TIME_ACTION',
}
interface SaveTrackerIdAction {
    type: 'SAVE_TRACKER_ID_ACTION',
    trackerId: number,
}
interface SaveUserNameAction {
    type: 'SAVE_USER_NAME_ACTION',
    userName: string,
}
interface SaveFileFilterAction {
    type: 'SAVE_FILE_FILTER_ACTION',
    fileFilter: string,
}
interface ResetFileListAction {
    type: 'RESET_FILE_LIST_ACTION',
}
interface ResetTrackerIdAction {
    type: 'RESET_TRACKER_ID_ACTION',
    trackerId: number,
}
interface ResetUserNameAction {
    type: 'RESET_USER_NAME_ACTION',
}
interface ResetFileFilterAction {
    type: 'RESET_FILE_FILTER_ACTION',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).

type KnownAction = GetFileListAction | SetFileListAction | MovePageFileListAction | ViewCountFileListAction | EditDeleteTimeAction
    | SaveTrackerIdAction | SaveUserNameAction | SaveFileFilterAction | ResetFileListAction | ResetTrackerIdAction | ResetUserNameAction | ResetFileFilterAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetFileList: (trackerid?: number, userName?: string, fileFilter?: string, count?: number, page?: number): AppThunkAction<GetFileListAction | SetFileListAction> => (dispatch, getState) => {
        const params = functions.GetParams(trackerid, undefined, userName, fileFilter, count, page);

        let fetchTask = functions.fetchTask('GetFileList', 'GET', params)
            .then(data => {
                if(data == null){
                    dispatch({ type: 'SET_FILE_LIST', fileList: null });
                } else {
                    data = data as File[];
                    if(data){
                        data.forEach((item:File) => {
                            if(item.RemoveFromDbTime){
                                item.RemoveFromDbTime = new Date(item.RemoveFromDbTime + "Z");
                            }
                        });
                    }
                    dispatch({ type: 'SET_FILE_LIST', fileList: data });
                }
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });

        addTask(fetchTask); 
        dispatch({ type: 'GET_FILE_LIST' });
    },
    EditDeleteTime: (fileId:number, dateTime: Date, isNeedDelete: boolean): AppThunkAction<EditDeleteTimeAction> => (dispatch, getState) => {
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
    SaveTrackerId: (trackerId: number) => <SaveTrackerIdAction>{ type: 'SAVE_TRACKER_ID_ACTION', trackerId: trackerId },
    ResetTrackerId:() => <ResetTrackerIdAction>{ type: 'RESET_TRACKER_ID_ACTION' },
    SaveUserName: (userName: string) => <SaveUserNameAction>{ type: 'SAVE_USER_NAME_ACTION', userName: userName },
    ResetUserName:() => <ResetUserNameAction>{ type: 'RESET_USER_NAME_ACTION' },
    SaveFileFilter:(fileFilter:string) => <SaveFileFilterAction>{ type: 'SAVE_FILE_FILTER_ACTION', fileFilter:fileFilter },
    ResetFileFilter:() => <ResetFileFilterAction>{ type: 'RESET_FILE_FILTER_ACTION' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: FileState = { fileListCountView: 10, fileListPage: 1, needGetData: false };

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
                        fileList: undefined,
                        needGetData: false,
                    }
                }
                let page = state.fileListPage < 1
                    ? 1
                    : state.fileListPage - 1;
                return {
                    ...state,
                    fileList: needEmptyList ? undefined : state.fileList,
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

        case 'VIEW_COUNT_FILE_LIST_ACTION':
            return {
                ...state,
                fileListCountView: action.count,
                fileListPage: 1,
                needGetData: true,
            }

        case 'EDIT_DELETE_TIME_ACTION':
            return {
                ...state,
                needGetData: true,
            }

        case 'SAVE_TRACKER_ID_ACTION':
            return {
                ...state,
                trackerId: action.trackerId,
                fileListPage: 1,
                needGetData: true,
            }

        case 'SAVE_USER_NAME_ACTION':
            return {
                ...state,
                userName: action.userName,
                fileListPage: 1,
                needGetData: true,                
            }

        case 'SAVE_FILE_FILTER_ACTION':
            return {
                ...state,
                fileFilter: action.fileFilter,
                fileListPage: 1,
                needGetData: true,
            }

        case 'RESET_FILE_FILTER_ACTION':
            return {
                ...state,
                fileFilter: undefined,
                fileListPage: 1,
                needGetData: true,
            }

        case 'RESET_TRACKER_ID_ACTION':
            return {
                ...state,
                trackerId: undefined,
                fileList: undefined,
                userName: undefined
            }

        case 'RESET_USER_NAME_ACTION':
            return {
                ...state,
                userName: undefined,
                fileListPage: 1,
                needGetData: true,
            }

        case 'RESET_FILE_LIST_ACTION':
            return unloadedState;

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};