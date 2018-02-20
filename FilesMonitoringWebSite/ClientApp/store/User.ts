import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface UserState {
    userType?: string,
    userName?: string,
    isBondUserName: boolean,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface GetUserInfoAction {
    type: 'GET_USER_INFO',
    userType?: string,
    userName?: string,
}
interface SignOutAction {
    type: 'SIGN_OUT',
}
interface BoundTrigger {
    type: 'BOUND_TRIGGER',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = GetUserInfoAction | SignOutAction | BoundTrigger;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const actionCreators = {
    GetUserInfo: (): AppThunkAction<GetUserInfoAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        let fetchTask = fetch(`/api/Authorization/GetUserInfo`, {
            method: 'GET',
            credentials: "same-origin"
        }).then(response => {
            if (response.status !== 200) return undefined;
            return response.json();
        }).then(data => {
            dispatch({ type: 'GET_USER_INFO', userType: data.userType, userName: data.userName });
        }).catch(err => {
            console.log('Error :-S in user', err);
        });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
    },
    SignOut: (): AppThunkAction<SignOutAction> => (dispatch, getState) => {
        let fetchTask = fetch(`/api/Authorization/SignOut`, {
            method: 'PUT',
            credentials: "same-origin"
        }).then(response => {
            dispatch({ type: 'SIGN_OUT' });
        }).catch(err => {
            console.log('Error :-S in user', err);
        });
        addTask(fetchTask);
    },
    BoundTrigger: () => <BoundTrigger>{ type: 'BOUND_TRIGGER' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: UserState = { userName: undefined, userType: undefined, isBondUserName: false };

export const reducer: Reducer<UserState> = (state: UserState, action: KnownAction) => {
    switch (action.type) {
        case 'SIGN_OUT':
            return unloadedState;

        case 'GET_USER_INFO':
            return {
                userName: action.userName,
                userType: action.userType,   
                isBondUserName: false,
            };

        case 'BOUND_TRIGGER':
            return {
                userName: state.userName,
                userType: state.userType,   
                isBondUserName: !state.isBondUserName,
            }

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};