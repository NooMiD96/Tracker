import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface ExceptionState {
    exceptionList: Exception[] | null,
    exceptionListCountView: number,
    exceptionListPage: number,
    needGetData: boolean,
    trackerId?: number,
    userName?: string,
}
export interface Exception{
    ExceptionInner: string,
    DateTime: Date,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface GetExceptionsAction {
    type: 'GET_EXCEPTIONS',
}
interface SetExceptionsAction {
    type: 'SET_EXCEPTIONS',
    exceptionList: Exception[] | null,
}
interface MovePageExceptionListAction {
    type: 'MOVE_PAGE_EXCEPTION_LIST_ACTION',
    prevOrNext: number,
}
interface ResetExceptionListAction {
    type: 'RESET_EXCEPTION_LIST_ACTION',
}
interface ViewCountExceptionListAction {
    type: 'VIEW_COUNT_EXCEPTION_LIST_ACTION',
    count: number,
}
interface SaveTrackerIdAction {
    type: 'SAVE_TRACKER_ID_ACTION',
    trackerId: number,
}
interface SaveUserNameAction {
    type: 'SAVE_USER_NAME_ACTION',
    userName: string,
}
interface DeleteUserNameAction {
    type: 'DELETE_USER_NAME_ACTION',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = GetExceptionsAction | SetExceptionsAction | MovePageExceptionListAction | ResetExceptionListAction |MovePageExceptionListAction | ViewCountExceptionListAction
    | SaveTrackerIdAction | SaveUserNameAction | DeleteUserNameAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetExceptionList: (trackerId?: number, userName?: string, count?: string | number, page?: string | number): AppThunkAction<GetExceptionsAction | SetExceptionsAction | SaveTrackerIdAction | SaveUserNameAction> => (dispatch, getState) => {
        let params = "";
        if (trackerId != null) {
            params = `?trackerid=${trackerId}`;
            if(count == null || page == null){
                dispatch({ type: 'SAVE_TRACKER_ID_ACTION', trackerId: trackerId });
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
                dispatch({ type: 'SAVE_USER_NAME_ACTION', userName:userName });
            }
        }
        if (count != null && page != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `count=${count}&page=${page}`;
        }
        let fetchTask = fetch(`/api/Tracker/GetExceptionList` + params, {
            method: 'GET',
            credentials: "same-origin"
        }).then(response => {
            if (response.status !== 200) return undefined;
            return response.json();
        }).then(data => {
            data = data as Exception[] | null;
            if(data){
                data.forEach((item:Exception) => {
                    item.DateTime = new Date(item.DateTime);
                });
            }
            dispatch({ type: 'SET_EXCEPTIONS', exceptionList: data as Exception[] | null });
        }).catch(err => {
            console.log('Error :-S in user', err);
        });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({type: 'GET_EXCEPTIONS'})
    },
    MovePageExceptionList: (prevOrNext: number) => <MovePageExceptionListAction>{ type: 'MOVE_PAGE_EXCEPTION_LIST_ACTION', prevOrNext: prevOrNext },
    ResetExceptionList: () => <ResetExceptionListAction>{ type: 'RESET_EXCEPTION_LIST_ACTION' },
    ViewCountExceptionList: (count: number) => <ViewCountExceptionListAction>{ type: 'VIEW_COUNT_EXCEPTION_LIST_ACTION', count: count },
    DeleteUserName: () => <DeleteUserNameAction>{ type: 'DELETE_USER_NAME_ACTION' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: ExceptionState = { exceptionList: null, exceptionListCountView: 10, exceptionListPage: 1, needGetData: false };

export const reducer: Reducer<ExceptionState> = (state: ExceptionState, action: KnownAction) => {
    switch (action.type) {
        case 'GET_EXCEPTIONS':
            return {
                ...state,
                needGetData: false,
            };

        case 'SET_EXCEPTIONS':
            if(action.exceptionList == null){
                if(state.exceptionListPage == 1){
                    return{
                        ...state,
                        exceptionList: null,
                        needGetData: false,
                    }
                }
                let page = state.exceptionListPage < 1
                    ? 1
                    : state.exceptionListPage - 1;
                return {
                    ...state,
                    exceptionList: state.exceptionList,
                    exceptionListPage: page,
                    needGetData: false,
                }
            }else{
                return {
                    ...state,
                    exceptionList: action.exceptionList,
                    needGetData: false,
                };
            }

        case 'MOVE_PAGE_EXCEPTION_LIST_ACTION':
            let newPage = state.exceptionListPage + action.prevOrNext;
            if (newPage < 1) {
                return {
                    ...state,
                    exceptionListPage: 1,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    exceptionListPage: newPage,
                    needGetData: true,
                }
            }

        case 'RESET_EXCEPTION_LIST_ACTION':
            return unloadedState;

        case 'VIEW_COUNT_EXCEPTION_LIST_ACTION':
            return{
                ...state,
                exceptionListCountView: action.count,
                exceptionListPage: 1,
                needGetData: true,
            }

        case 'SAVE_TRACKER_ID_ACTION':
            return {
                ...state,
                trackerId: action.trackerId,
                exceptionListPage: 1,
            }

        case 'SAVE_USER_NAME_ACTION':
            return{
                ...state,
                userName: action.userName,
                exceptionListPage: 1,
            }

        case 'DELETE_USER_NAME_ACTION':
            return {
                ...state,
                userName: undefined,
                needGetData: true,
                exceptionListPage: 1,
            }
            
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};