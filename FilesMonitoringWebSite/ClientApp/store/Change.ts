import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { functions } from '../func/fetchHelper';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface ChangeState {
    changeList: Change[] | null,
    changeListCountView: number,
    changeListPage: number,
    needGetData: boolean,
    fileId?:number,
}
export interface Change {
    Id: number,
    EventName: ChangeEvents,
    DateTime: Date,
    Content: string,
    UserName: string,
    OldFullName: string | null,
    OldName: string | null,
}
export enum ChangeEvents {
    Changed,
    Created,
    Deleted,
    Renamed,
    Moved,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.
interface GetChangeListAction {
    type: 'GET_CHANGE_LIST',
}
interface SetChangeListAction {
    type: 'SET_CHANGE_LIST',
    changeList: Change[] | null,
}
interface MovePageChangeListAction {
    type: 'MOVE_PAGE_CHANGE_LIST_ACTION',
    prevOrNext: number,
}
interface SaveFileIdAction {
    type: 'SAVE_FILE_ID_ACTION',
    fileId?: number,
}
interface ResetChangeListAction {
    type: 'RESET_CHANGE_LIST_ACTION',
}
interface ViewCountChangeListAction {
    type: 'VIEW_COUNT_CHANGE_LIST_ACTION',
    count: number,
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).

type KnownAction = GetChangeListAction | SetChangeListAction | MovePageChangeListAction
    | ResetChangeListAction | ViewCountChangeListAction | SaveFileIdAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetChangeList: (fileId?: number, count?: string | number, page?: string | number): AppThunkAction<GetChangeListAction | SetChangeListAction | SaveFileIdAction> => (dispatch, getState) => {
        let params = "";
        if (fileId != null) {
            params = `?fileId=${fileId}`;
        }
        if (count != null && page != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `count=${count}&page=${page}`;
        }
        // Only load data if it's something we don't already have (and are not already loading)
        let fetchTask = functions.fetchTask('GetChangeList', 'GET', params)
            .then(data => {
                if(data == null){
                    dispatch({ type: 'SET_CHANGE_LIST', changeList: null });
                }else{
                    data = data as Change[] | null;
                    if(data){
                        data.forEach((item:Change) => {
                            item.DateTime = new Date(item.DateTime);
                        });
                    }
                    dispatch({ type: 'SET_CHANGE_LIST', changeList: data as Change[] | null });
                }
                
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'SAVE_FILE_ID_ACTION', fileId: fileId });
        dispatch({ type: 'GET_CHANGE_LIST' });
    },
    MovePageChangeList: (prevOrNext: number) => <MovePageChangeListAction>{ type: 'MOVE_PAGE_CHANGE_LIST_ACTION', prevOrNext: prevOrNext },
    ResetChangeList: () => <ResetChangeListAction>{ type: 'RESET_CHANGE_LIST_ACTION' },
    ViewCountChangeList: (count: number) => <ViewCountChangeListAction>{ type: 'VIEW_COUNT_CHANGE_LIST_ACTION', count: count },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: ChangeState = { changeList: null, changeListCountView: 10, changeListPage: 1, needGetData: false };

export const reducer: Reducer<ChangeState> = (state: ChangeState, action: KnownAction) => {
    switch (action.type) {
        case 'GET_CHANGE_LIST':
            return { 
                ...state,
                needGetData: false,
            };

        case 'SET_CHANGE_LIST':
            if (action.changeList == null) {
                let page = state.changeListPage < 1
                    ? 1
                    : state.changeListPage - 1;
                return {
                    ...state,
                    changeListPage: page,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    changeList: action.changeList,
                    needGetData: false,
                };
            }

        case 'MOVE_PAGE_CHANGE_LIST_ACTION':
            let newPage = state.changeListPage + action.prevOrNext;
            if (newPage < 1) {
                return {
                    ...state,
                    changeListPage: 1,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    changeListPage: newPage,
                    needGetData: true,
                }
            }

        case 'SAVE_FILE_ID_ACTION':
            return {
                ...state,
                fileId: action.fileId,
            }

        case 'RESET_CHANGE_LIST_ACTION':
            return unloadedState;

        case 'VIEW_COUNT_CHANGE_LIST_ACTION':
            return {
                ...state,
                changeListCountView: action.count,
                changeListPage: 1,
                needGetData: true,
            };

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};