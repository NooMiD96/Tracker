import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { functions } from '../func/fetchHelper';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface TrackerState {
    trackerList: Tracker[] | null,
    trackerListCountView: number,
    trackerListPage: number,
    needGetData: boolean,
}
export interface Tracker {
    TrackerId: number,
    UserName?: string,
    IsCanAuthohorization?: boolean,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface GetTrackerListAction {
    type: 'GET_TRACKER_LIST',
}
interface SetTrackerListAction {
    type: 'SET_TRACKER_LIST',
    trackerList: Tracker[] | null,
}
interface MovePageTrackerListAction {
    type: 'MOVE_PAGE_TRACKER_LIST_ACTION',
    prevOrNext: number,
}
interface ResetTrackerListAction {
    type: 'RESET_TRACKER_LIST_ACTION',
    prevOrNext: number,
}
interface ViewCountTrackerListAction {
    type: 'VIEW_COUNT_TRACKER_LIST_ACTION',
    count: number,
}
interface ChangeAccessUserAction {
    type: 'CHANGE_ACCESS_USER_ACTION',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).

type KnownAction = GetTrackerListAction | SetTrackerListAction | ResetTrackerListAction | ViewCountTrackerListAction
    | MovePageTrackerListAction | ChangeAccessUserAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetTrackerList: (needUsersName: boolean, count?: string | number, page?: string | number): AppThunkAction<GetTrackerListAction | SetTrackerListAction> => (dispatch, getState) => {
        let params = `?needUsersName=${needUsersName}`;
        if (count != null && page != null) {
            params += `&count=${count}&page=${page}`;
        }
        // Only load data if it's something we don't already have (and are not already loading)
        let fetchTask = functions.fetchTask('GetTrackerList', 'GET', params)
            .then(data => {
                dispatch({ type: 'SET_TRACKER_LIST', trackerList: data as Tracker[] | null });
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'GET_TRACKER_LIST' });
    },
    ChangeAccessUser: (trackerId: string | number, userName: string | undefined, password?: string): AppThunkAction<ChangeAccessUserAction> => (dispatch, getState) => {
        let params = JSON.stringify({trackerId: trackerId, userName: userName, password: password})

        let fetchTask = functions.fetchAdminTask('EditUser', 'PUT', params)
            .then(data => {
                dispatch({ type: 'CHANGE_ACCESS_USER_ACTION' });
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
    },
    MovePageTrackerList: (prevOrNext: number) => <MovePageTrackerListAction>{ type: 'MOVE_PAGE_TRACKER_LIST_ACTION', prevOrNext: prevOrNext },
    ResetTrackerList: () => <ResetTrackerListAction>{ type: 'RESET_TRACKER_LIST_ACTION' },
    ViewCountTrackerList: (count: number) => <ViewCountTrackerListAction>{ type: 'VIEW_COUNT_TRACKER_LIST_ACTION', count: count },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: TrackerState = { trackerList: null, trackerListCountView: 10, trackerListPage: 1, needGetData: false };

export const reducer: Reducer<TrackerState> = (state: TrackerState, action: KnownAction) => {
    switch (action.type) {
        case 'GET_TRACKER_LIST':
            return {
                ...state,
                needGetData: false,
            };

        case 'SET_TRACKER_LIST':
            if (action.trackerList == null) {
                if (state.trackerListPage == 1) {
                    return {
                        ...state,
                        needGetData: false,
                    }
                }
                let page = state.trackerListPage < 1
                    ? 1
                    : state.trackerListPage - 1;
                return {
                    ...state,
                    trackerListPage: page,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    trackerList: action.trackerList,
                    needGetData: false,
                };
            }

        case 'MOVE_PAGE_TRACKER_LIST_ACTION':
            let newPage = state.trackerListPage + action.prevOrNext;
            if (newPage < 1) {
                return {
                    ...state,
                    trackerListPage: 1,
                    needGetData: false,
                }
            } else {
                return {
                    ...state,
                    trackerListPage: newPage,
                    needGetData: true,
                }
            }

        case 'RESET_TRACKER_LIST_ACTION':
            return unloadedState;

        case 'VIEW_COUNT_TRACKER_LIST_ACTION':
            return {
                ...state,
                trackerListCountView: action.count,
                trackerListPage: 1,
                needGetData: true,
            };

        // case 'CREATE_NEW_USER_ACTION':
        //     return {
        //         ...state,
        //         needGetData: true,
        //     }

        // case 'DELETE_USER_ACTION':
        //     return {
        //         ...state,
        //         needGetData: true,
        //     }

        // case 'DELETE_TRACKER_ACTION':
        //     return {
        //         ...state,
        //         needGetData: true,
        //     }

        case 'CHANGE_ACCESS_USER_ACTION':
            return {
                ...state,
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